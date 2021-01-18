using System;
using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class SwitchFlowApi<T> : CommonFlowApi<ISwitchDoFlowApi<T>, ISwitchDoCloseFlowApi<T>, ISwitchDefaultCloseFlowApi<T>, ISwitchDoCloseFlowApi<T>, ISwitchDefaultCloseFlowApi<T>>,
    ISwitchFlowApi<T>, ISwitchCaseFlowApi<T>, ISwitchCaseCaseFlowApi<T>, ISwitchDefaultFlowApi<T>
    , ISwitchDoCloseFlowApi<T>, ISwitchDefaultCloseFlowApi<T>
    {
        private readonly T _parentBlockPath;
        private readonly IContainerFlow _parentcontainerFlow; 
        private readonly SwitchCaseContainerFlow _switchCaseContainerFlow;

        internal SwitchFlowApi(T parentBlockPath, IContainerFlow parentcontainerFlow,
            SwitchCaseContainerFlow switchCaseContainerFlow, CodeFlowExecutionConfig config) : base(switchCaseContainerFlow, config)
        {
            _parentBlockPath = parentBlockPath;
            _parentcontainerFlow = parentcontainerFlow;  
            _switchCaseContainerFlow = switchCaseContainerFlow;
            Set5ReferenceFlowApi(this);
        } 

        ISwitchCaseCaseFlowApi<T> ISwitchDoFlowApi<T>.Case(Func<object, object[], bool> switchSelector)
        {
            _switchCaseContainerFlow.Case(switchSelector);
            return this;
        }

        public ISwitchDefaultFlowApi<T> Default()
        {
            _switchCaseContainerFlow.Default();
            return this;
        }

        ISwitchCaseFlowApi<T> ISwitchFlowApi<T>.Case(Func<object, object[], bool> switchSelector)
        {
            _switchCaseContainerFlow.Case(switchSelector);
            return this;
        }

        T ISwitchDefaultCloseFlowApi<T>.CloseSwitch()
        {
            return CloseSwitchInternal();
        }

        public T CloseSwitch()
        {
            return CloseSwitchInternal();
        }

        T CloseSwitchInternal()
        {
            if (!_switchCaseContainerFlow.IsAllContainerClosed()) // close previous case or default statements
            {
                _switchCaseContainerFlow.CloseContainer();
            }
            _parentcontainerFlow.CloseContainer(); // close swith container block on parent 
            return _parentBlockPath;
        }
    }
}
 