using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.Filter;
using aceryansoft.codeflow.model.Middlewares;

namespace aceryansoft.codeflow.model.Config
{
    public  interface ICodeFlowExecutionConfig
    { 
        ICodeFlowExecutionConfig UseMiddleware(Action<ICodeFlowContext, Action> middleware);
        ICodeFlowExecutionConfig UseMiddleware<T>(string instanceName = "") where T : ICodeFlowMiddleWare;
        ICodeFlowExecutionConfig UseMiddleware(ICodeFlowMiddleWare middleware);


        ICodeFlowExecutionConfig UseActivityFilter(Func<ICodeFlowContext, object[]
            , Func<ICodeFlowContext, object[], IExecutionLog>, IExecutionLog> activityFilter);
        ICodeFlowExecutionConfig UseActivityFilter<T>(string instanceName = "") where T : ICodeFlowActivityFilter;
        ICodeFlowExecutionConfig UseActivityFilter(ICodeFlowActivityFilter instance);


       ICodeFlowExecutionConfig WithContext(Func<ICodeFlowContext> contextProvider);
        ICodeFlowExecutionConfig WithServiceResolver(Func<Type, string, object> serviceResolver);

        ICodeFlowExecutionConfig RerunMode(bool isRerun);
        ICodeFlowExecutionConfig RestorePointId(string restorePointId);
    }

}