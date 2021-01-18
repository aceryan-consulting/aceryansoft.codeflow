using Ninject;
using System;
using System.Text;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Activities;
using System.Linq;
using aceryansoft.codeflow.model.FlowApi;
using System.Collections.Concurrent;

namespace aceryan.codeflow.samples.ShowCase
{
    public class CvarCodeFlow
    {
        private readonly Func<Type, string, object> _serviceResolver;

        public CvarCodeFlow(string computeEngine = "")
        {
            var kernel = BuildDiContainer(computeEngine);
            _serviceResolver = (typ, str) => string.IsNullOrEmpty(str) 
                                           ? kernel.Get(typ) : kernel.Get(typ, str);
        }

        public void Run(string requestId="", bool rerunMode=false,string restorePoint="")
        {            
            var cvarcodeFlow = new CodeFlow();
            cvarcodeFlow.StartNew(cfg => {
                cfg.WithContext(() =>
                {
                    var context = new CvarContext() { Criteria = new SearchCriteria() { Filter = "otc trades" } };
                    context.RequestId = string.IsNullOrEmpty(requestId) ? context.RequestId : requestId;
                    return context;
                })
                .UseMiddleware<IExceptionHandlingMiddleware>()
                .UseActivityFilter<IAppLogActivityFilter>()
                .RerunMode(rerunMode).RestorePointId(restorePoint).WithServiceResolver(_serviceResolver);
            })
            .Do<ICodeFlowActivity>("LoadTrades")
            .If((ctx,inputs)=> ctx.GetCollection<Trade>("Trades").Any(x=>x.IsOption))
                .Do<ILoadTradesVolatility>()
            .Close()
            .Do<IExportOptimalProgramsForExoticDeals>()
            .Do<IComputeMatchingBetweenTradesAndContracts>()
            .Parallel()
                .CallCodeFlow(ExportLegalEntitiesInfos)
                .Do<IExportStaticData>()
                .Do<IExportUnderlyings>()
                .Do<IExportFixings>()
            .Close()
            .RestorePoint("restore.after.export", SaveRestorePointContextState, LoadRestorePointContextState)
            .Switch((ctx, inputs) => ctx.GetValue<string>("SimulationAlgorithm"))
                .Case((val, inputs) => val?.ToString()== "MonteCarlo")
                    .Call((ctx, inputs) =>
                    {
                        //Compute diffusion on each riskfactor, default date, scenario with MonteCarlo algorithm
                    })
                .Case((val, inputs) => val?.ToString() == "LossCalculation")
                    .Call((ctx, inputs) =>
                    {
                        //Compute diffusion on each riskfactor, default date, scenario with LossCalculation algorithm
                    })
            .CloseSwitch()
            .RestorePoint("restore.after.diffusion", SaveRestorePointContextState, LoadRestorePointContextState)
            .ForEach((ctx, inputs)=> ctx.GetCollection<CtyComputeParameter>("CtyComputeParameter").OfType<object>().ToList(),packetSize:10)
                .AsParallel()
                    .Function<ICodeFlowActivityFunction>("CvarComputerFunction")
                        .WithArg<ConcurrentBag<SharedComputeInputs>>("shareinputs",(ctx, inputs) => ctx.As<CvarContext>().SharedInputs)
                        .WithArg<string>("Currency",(ctx, inputs) => "EUR")
                        .WithArg<decimal>("Threshold",(ctx, inputs) => 0.95M)
                        .WithResult<CtyComputeResult>((ctx, inputs,res) => ctx.GetCollection<CtyComputeResult>("CtyResults").Add(res))
                    .Close()
                .Close()
            .CloseForEach()
            .Call((ctx, inputs) =>
            {
                //Save computation results to database ... 
            })
            .Close();

            cvarcodeFlow.Execute(); 
        }

        private StandardKernel BuildDiContainer(string computeEngine = "")
        {
            var kernel = new StandardKernel();
            kernel.Bind<IExceptionHandlingMiddleware>().To<ExceptionHandlingMiddleware>();
            kernel.Bind<IAppLogActivityFilter>().To<AppLogActivityFilter>();

            kernel.Bind<IMarkToMarketService>().To<MarkToMarketService>();
            kernel.Bind<ITradeSearchService>().To<TradeSearchService>();
            kernel.Bind<ICodeFlowActivity>().To<LoadTradesActivity>().Named("LoadTrades");
            // register all activities, services and functions ... 

            kernel.Bind<ICodeFlowActivityFunction>().To<LegacyCvarComputerFunction>()
                  .When(req => computeEngine == "legacy").Named("CvarComputerFunction");

            kernel.Bind<ICodeFlowActivityFunction>().To<NewCvarComputerFunction>()
                  .When(req => computeEngine != "legacy").Named("CvarComputerFunction");

            return kernel;
        }

        private void ExportLegalEntitiesInfos(ICallFlowApi blockPath)
        {
            blockPath
                .Do<ICodeFlowActivity>("ExportLegalEntities")
                .Do<ICodeFlowActivity>("ExportCreditLimits")
                .Do<ICodeFlowActivity>("ExportContracts")
                .Do<ICodeFlowActivity>("ExportMarginCalls")
            .Close();
        }
        
        private void SaveRestorePointContextState(string reqId, string pointId, object[] inputs, ICodeFlowContext ctx)
        { 
            //save execution context to any persistent store
        }

        private ICodeFlowContext LoadRestorePointContextState(string reqId, string pointId)
        {
            //load execution context from any persistent store
            return new CvarContext();
        }      
    }
}
