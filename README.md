# aceryansoft.codeflow
![.Net](https://github.com/aceryan-consulting/aceryansoft.codeflow/workflows/.NET/badge.svg)

aceryansoft.codeflow is a C# simple, fluent and feature driven programming framework targeting .Net standard 2.0 and .Net framework 4.5.2 .
It helps design and write complex and long running processes by features. 

First think about your process as an ordered collection of features, then decompose each feature into smaller and less complex sub-features 
, finally decompose each sub-feature into loosely-coupled activities small enough to be delivered during few days.
By designing your code with this mind set and using our fluent api combined with dependency injection, you should write code easier to read, reuse and test. 

## Showcase
let's read this sample code written with aceryansoft.codeflow to compute some financial indicator (CVAR: Credit value at risk).
 
```c#
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

```

Don't you think that this code is simple and clear ? 

Don't you want to join projects were legacy code is written as this showcase ? 


## Contributing
All contribution are welcome, please read the [Code of conduct](https://github.com/aceryan-consulting/aceryansoft.codeflow/blob/develop/CODE_OF_CONDUCT.md) and contact the author.

## Documentation
Please read [wiki](https://github.com/aceryan-consulting/aceryansoft.codeflow/wiki).

## License
This project is licensed under the terms of the Apache-2.0 License. 
Please read [LICENSE](LICENSE.md) file for license rights and limitations.
 
