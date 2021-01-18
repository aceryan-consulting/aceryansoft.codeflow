using System;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.model.FlowApi
{
    public interface IFunctionFlowApi<T>
    {
        IFunctionFlowApi<T> WithArg<TArg>(string propertyName, Func<ICodeFlowContext,object[], TArg> argValueProvider);
        ICloseFunctionFlowApi<T> WithResult<TResult>(Action<ICodeFlowContext, object[], TResult> resultCallback);
    }

    public interface ICloseFunctionFlowApi<T>
    {
        T Close();
    }
}