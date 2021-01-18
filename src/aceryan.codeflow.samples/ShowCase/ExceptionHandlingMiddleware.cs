using System;
using aceryansoft.codeflow.model.Middlewares;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Filter;

namespace aceryan.codeflow.samples.ShowCase
{
    public interface IExceptionHandlingMiddleware : ICodeFlowMiddleWare
    {

    }
    public class ExceptionHandlingMiddleware : IExceptionHandlingMiddleware
    { 
        public void Execute(ICodeFlowContext context, Action next)
        {
            try
            { 
                next();
            }
            catch(Exception ex)
            {
                // log and report exceptions 
            }             
        }
    }


    public interface IAppLogActivityFilter : ICodeFlowActivityFilter
    {

    }

    public class AppLogActivityFilter : IAppLogActivityFilter
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
}
