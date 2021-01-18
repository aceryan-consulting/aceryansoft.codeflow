using System;
using System.Collections.Generic;
using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class LoopFlowApi<T, TRes> : CommonFlowApi<T>, ILoopFlowApi<T, TRes>
    {
        protected TRes _referenceFlowApiRes;
        internal LoopFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config) : base(containerFlow, config)
        {
        }

        public void SetReferenceFlowApiWithResult<TData>(TData referenceFlowApi) where TData : T, TRes
        {
            _referenceFlowApi = referenceFlowApi;
            _referenceFlowApiRes = referenceFlowApi;
        }

        public TRes If(Func<ICodeFlowContext, object[], bool> condition, Action<FlowContainerConfig> configAction = null)
        {
            var ifContainer = new IfContainerFlow(condition);
            ifContainer.WithConfig(configAction);
            _containerFlow.AddContainer(ifContainer);
            return _referenceFlowApiRes;
        }

        public TRes While(Func<ICodeFlowContext, object[], bool> condition, Action<FlowContainerConfig> configAction = null)
        {
            var ifContainer = new WhileContainerFlow(condition);
            ifContainer.WithConfig(configAction);
            _containerFlow.AddContainer(ifContainer);
            return _referenceFlowApiRes;
        }

        public ISwitchFlowApi<TRes> Switch(Func<ICodeFlowContext, object[], object> switchExpression)
        {
            var switchContainer = new SwitchCaseContainerFlow(switchExpression);
            _containerFlow.AddContainer(switchContainer);
            return new SwitchFlowApi<TRes>(_referenceFlowApiRes, _containerFlow, switchContainer, _config);
        }

        public IForEachFlowApi<TRes> ForEach(Func<ICodeFlowContext, object[], List<object>> iterator, int packetSize = 0)
        {
            return new ForEachFlowApi<TRes>(_referenceFlowApiRes, _containerFlow, iterator, _config, packetSize);
        }
    }
}