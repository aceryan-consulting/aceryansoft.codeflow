using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.model.FlowApi
{
    public interface ICommonFlowApi<T>
    {
        T Do(ActivityDelegate activity);
        T Do(ICodeFlowActivity instance);
        T Do<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivity;

        T Call(ActivityCallDelegate activity);
        T Call(ICodeFlowActivityCall instance);
        T Call<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivityCall;

        IFunctionFlowApi<T> Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null);
        IFunctionFlowApi<T> Function(ICodeFlowActivityFunction instance);
        IFunctionFlowApi<T> Function<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivityFunction;
    }

    public interface ILoopFlowApi<T,TRes> : ICommonFlowApi<T>
    {
        TRes If(Func<ICodeFlowContext, object[], bool> condition, Action<FlowContainerConfig> configAction = null);
        TRes While(Func<ICodeFlowContext, object[], bool> condition, Action<FlowContainerConfig> configAction = null);
        ISwitchFlowApi<TRes> Switch(Func<ICodeFlowContext, object[], object> switchExpression);
        IForEachFlowApi<TRes> ForEach(Func<ICodeFlowContext, object[], List<object>> iterator, int packetSize = 0);
    }

    public interface IFlowApi<T> : ILoopFlowApi<T,T>
    {
        T CallCodeFlow(Action<ICallFlowApi> codeFlowBlock);
        T Parallel();
        T Close();
    }

    public interface ICallFlowApi : IFlowApi<ICallFlowApi>
    {

    }

    public interface ICodeFlowApi : IFlowApi<ICodeFlowApi>
    {
        ICodeFlowApi RestorePoint(string restorePointId, Action<string, string, object[], ICodeFlowContext> contextSaver
            , Func<string, string, ICodeFlowContext> contextProvider);
    }
}
