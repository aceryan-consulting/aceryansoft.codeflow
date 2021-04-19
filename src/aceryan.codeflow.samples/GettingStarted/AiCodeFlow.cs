using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Filter;
using aceryansoft.codeflow.model.FlowApi;
using aceryansoft.codeflow.model.Middlewares;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aceryan.codeflow.samples.GettingStarted
{
    public class AiContext : CodeFlowContext
    {
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public decimal LoyaltyThreshold { get; set; }
        public decimal SpendingThreshold { get; set; }
        public int CleanAttempts { get; set; }
    }

    public enum Gender
    {
        Male, 
        Female
    }

    public class Customer
    {
        public string CustomerId { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; } 
        public decimal AnnualIncome { get; set; } 
        public int SpendingScore { get; set; } 
        public bool IsCleaned { get; set; } 
    }

    public class Order
    {
        public string CustomerId { get; set; }
        public string OrderId { get; set; } 
        public string Category { get; set; } 
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } 
        public decimal Cost { get; set; } 
    }


    public class AiCodeFlowWrapper
    {
        public void Run(string dataSource)
        {
            var kernel = BuildDiContainer();
            Func<Type, string, object> serviceResolver = (typ, str) => string.IsNullOrEmpty(str)
                                           ? kernel.Get(typ) : kernel.Get(typ, str);

            var aicodeFlow = new CodeFlow();
            aicodeFlow.StartNew(cfg => {
                cfg.WithContext(() => new AiContext() { CleanAttempts=3 })
                .UseMiddleware<IExceptionHandlingMiddleware>()  
                .UseActivityFilter<ILogActivityFilter>()  
                .WithServiceResolver(serviceResolver);
            })
            .Switch((ctx, inputs) => dataSource)
                .Case((val, inputs) => val?.ToString() == "external")
                    .Do<ILoadExternalCustomerActivity>()
                .Case((val, inputs) => val?.ToString() == "reuters")
                    .Do<ILoadReutersCustomerActivity>()
                .Default()
                    .Do<ILoadCustomerActivity>()
            .CloseSwitch()
            .Do<ILoadCustomerActivity>()  
            .Do<ILoadCustomerOrdersActivity>()
            .While((ctx, inputs) => ctx.GetValue<int>("CleanAttempts")>0 && ctx.GetCollection<Customer>("Customers").Any(x=>!x.IsCleaned) )
                .CallCodeFlow(FillAndCleanCustomerData)
                .Call((ctx, inputs)=>
                {
                    var attempts = ctx.GetValue<int>("CleanAttempts");
                    ctx.SetValue<int>("CleanAttempts", attempts - 1);
                })
            .Close()
            .ForEach((ctx, inputs) => ctx.GetCollection<Customer>("Customers").OfType<object>().ToList(), packetSize: 10) // packetSize is optional and no iteration split by package logic is applied when not specified
                //.AsSequence() you can use this to execute contained activities in sequence 
                .AsParallel() // execute contained activities in parallel and by packet of (packetSize = 10)
                     .Call((ctx, inputs) =>
                     {
                         //update the SpendingScore of the current customer = inputs[0] 
                     })
                .Close()
            .CloseForEach()
            .Function<ISpendingThresholdFunction>()
                .WithArg<string>("Country", (ctx, inputs) => "France")
                .WithArg<decimal>("AdjustmentValue", (ctx, inputs) => 1.85M)
                .WithResult<decimal>((ctx, inputs, res) => ctx.SetValue<decimal>("SpendingThreshold", res))
            .Close()
            .If((ctx, inputs) => ctx.GetValue<decimal>("SpendingThreshold") < 1)
                 .Call((ctx, inputs) =>
                 {
                    //send alert to the marketing team
                 })
            .Close()
            .Call((ctx, inputs) =>
             {
                 //save results to data base for further analysis 
             })
            .Close();

            aicodeFlow.Execute();
        }
      
        private void FillAndCleanCustomerData(ICallFlowApi callBlock)
        {
            callBlock.Do<IFillMissingDataWithMeanMedianActivity>()
            .Do<IFillMissingDataWithMostFrequentActivity>()
            .Do<IFillMissingDataWithKNNActivity>()
            .CallCodeFlow(CleanCustomerData); // just to show that CallCodeFlow syntax allow inner calls
        }

        private void CleanCustomerData(ICallFlowApi callBlock)
        {
            callBlock.Do<IRemoveIncoherentDataActivity>();
        }

        private StandardKernel BuildDiContainer()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IExceptionHandlingMiddleware>().To<ExceptionHandlingMiddleware>();
            kernel.Bind<ILogActivityFilter>().To<LogActivityFilter>();            
            
            kernel.Bind<ICustomerAndOrderService>().To<SampleCustomerAndOrderService>();
            kernel.Bind<ILoadCustomerActivity>().To<LoadCustomerActivity>();
            kernel.Bind<ILoadCustomerOrdersActivity>().To<LoadCustomerOrdersActivity>();


            kernel.Bind<IAwesomeSpendingThresholdComputer>().To<AwesomeSpendingThresholdComputer>();
            kernel.Bind<ISpendingThresholdFunction>().To<SpendingThresholdFunction>();

            // register other dependencies ... 
            return kernel;
        }
    }

    public interface IExceptionHandlingMiddleware : ICodeFlowMiddleWare
    {

    }


    public class ExceptionHandlingMiddleware : IExceptionHandlingMiddleware
    {
        public void Execute(ICodeFlowContext context, Action next)
        {
            try
            {
                // run code before the execution pipeline
                next();
                // run code after the execution pipeline
            }
            catch (Exception ex)
            {
                //log exception and execute some logic
            }
        }
    }

    public interface ILogActivityFilter : ICodeFlowActivityFilter
    {

    }

    public class LogActivityFilter : ILogActivityFilter
    {
        public void OnActivityExecuting(ICodeFlowContext context, params object[] inputs)
        {
            //run code before each activity
        }

        public void OnActivityExecuted(ICodeFlowContext context, IExecutionLog res, params object[] inputs)
        {
            // log each activity details : res.Status, res.ActivityName, res.StartDate, res.Duration , res.Error , res.ErrorCode,  etc ... 
            //run code after each activity
        }
    }

    public interface IFillMissingDataWithMeanMedianActivity : ICodeFlowActivity
    {

    }
    public interface IFillMissingDataWithMostFrequentActivity : ICodeFlowActivity
    {

    }
    public interface IFillMissingDataWithKNNActivity : ICodeFlowActivity
    {

    }

    public interface IRemoveIncoherentDataActivity : ICodeFlowActivity
    {

    }
    public interface ILoadCustomerActivity : ICodeFlowActivity
    {

    }

    public interface ILoadExternalCustomerActivity : ICodeFlowActivity
    {

    }
   
    public interface ILoadReutersCustomerActivity : ICodeFlowActivity
    {

    }
     
    public interface ILoadCustomerOrdersActivity : ICodeFlowActivity
    {

    }

    public class LoadCustomerOrdersActivity : ILoadCustomerOrdersActivity
    {
        private readonly ICustomerAndOrderService _customerAndOrderService;

        public LoadCustomerOrdersActivity(ICustomerAndOrderService customerAndOrderService)
        {
            _customerAndOrderService = customerAndOrderService;
        }
        public IExecutionContext Execute(ICodeFlowContext context, params object[] inputs)
        {
            var orders = _customerAndOrderService.GetCustomerOrders();
            context.SetCollection<Order>("Orders", orders);
            return new ExecutionContext() { ActivityName = "LoadOrdersActivity", Status = Status.Succeeded };
        }
    }

    public class LoadCustomerActivity : ILoadCustomerActivity
    {
        private readonly ICustomerAndOrderService _customerAndOrderService;

        public LoadCustomerActivity(ICustomerAndOrderService customerAndOrderService)
        {
            _customerAndOrderService = customerAndOrderService;
        }
        public IExecutionContext Execute(ICodeFlowContext context, params object[] inputs)
        {
            var customers = _customerAndOrderService.GetCustomers();
            context.SetCollection<Customer>("Customers", customers);
            return new ExecutionContext() { ActivityName = "LoadCustomerActivity", Status = Status.Succeeded };
        }
    }

    public interface ICustomerAndOrderService
    {
        List<Customer> GetCustomers();
        List<Order> GetCustomerOrders();
    }

    public class SampleCustomerAndOrderService : ICustomerAndOrderService
    {
        public List<Order> GetCustomerOrders()
        {
            return new List<Order>();
        }

        public List<Customer> GetCustomers()
        {
            return new List<Customer>();
        }
    }

   
    public class SpendingThresholdFunction : ISpendingThresholdFunction
    {
        private readonly IAwesomeSpendingThresholdComputer _awesomeSpendingThresholdComputer;

        // Country and  AdjustmentValue value are automatically filled when creating the function class
        public string Country { get; set; }  
        public decimal AdjustmentValue { get; set; } 

        public SpendingThresholdFunction(IAwesomeSpendingThresholdComputer awesomeSpendingThresholdComputer)
        {
            _awesomeSpendingThresholdComputer = awesomeSpendingThresholdComputer;
        }

        public object Execute(ICodeFlowContext context, params object[] inputs)
        {
            // Another key point here is that function parameters (Country=inputs[0], AdjustmentValue=inputs[1]) are also available in inputs array.  
            // we will talk about Foreach loop later, but for now remember that if our function is called in a foreach loop 
            // inputs[0 ... n] will contains loop values and  inputs[n+1 ... n+p] will contains parameter values
            var customers = context.GetCollection<Customer>("Customers");
            var orders = context.GetCollection<Order>("Orders");
            var formated3Ddata = FormatInputsAs3DBeforeComputation(customers, orders);
            return _awesomeSpendingThresholdComputer.Compute(Country, AdjustmentValue,formated3Ddata);   
        }

        private object[,,] FormatInputsAs3DBeforeComputation(List<Customer> customers, List<Order> orders)
        {
            //todo write 3D formating logic 
            return new object[,,] { };
        }
    }
    public interface ISpendingThresholdFunction : ICodeFlowActivityFunction
    {

    }

    public interface IAwesomeSpendingThresholdComputer
    {
        decimal Compute(string country, decimal adjustmentValue, object[,,] dataMatrix);
    }

    public class AwesomeSpendingThresholdComputer : IAwesomeSpendingThresholdComputer
    {
        public decimal Compute(string country, decimal adjustmentValue, object[,,] dataMatrix)
        {
            throw new NotImplementedException();
        }
    }

}
