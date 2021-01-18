using System;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.model.FlowApi
{
    public interface ISwitchFlowApi<T>
    {
        ISwitchCaseFlowApi<T> Case(Func<object, object[], bool> switchSelector);
    }

    public interface ISwitchCaseFlowApi<T> : ICommonFlowApi<ISwitchDoFlowApi<T>>
    { }

    public interface ISwitchCaseCaseFlowApi<T> : ICommonFlowApi<ISwitchDoCloseFlowApi<T>>
    {  }

    public interface ISwitchDefaultFlowApi<T> : ICommonFlowApi<ISwitchDefaultCloseFlowApi<T>>
    {   }

    public interface ISwitchDoFlowApi<T> : ICommonFlowApi<ISwitchDoCloseFlowApi<T>>
    { 
        ISwitchCaseCaseFlowApi<T> Case(Func<object,object[], bool> switchSelector);
        ISwitchDefaultFlowApi<T> Default();
    }

    public interface ISwitchDoCloseFlowApi<T> : ISwitchDoFlowApi<T>
    {
        T CloseSwitch();
    }

    public interface ISwitchDefaultCloseFlowApi<T> : ICommonFlowApi<ISwitchDefaultCloseFlowApi<T>>
    {
        T CloseSwitch();
    }

}