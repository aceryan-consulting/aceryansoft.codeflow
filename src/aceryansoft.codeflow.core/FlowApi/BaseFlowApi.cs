using System;
using System.Collections.Generic;
using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class BaseFlowApi<T> : LoopFlowApi<T,T> , IFlowApi<T>
    { 
        internal BaseFlowApi(SequenceContainerFlow sequenceContainerFlow, CodeFlowExecutionConfig config) : base(sequenceContainerFlow, config)
        {  
        }
      
        public T CallCodeFlow(Action<ICallFlowApi> codeFlowBlock)
        {
            var codeblock = new CallFlowApi( new SequenceContainerFlow(), _config);
            codeFlowBlock?.Invoke(codeblock);
            codeblock.AddToParentContainer(_containerFlow);
            return _referenceFlowApi;
        }

        protected void AddToParentContainer(IContainerFlow parentSequenceContainerFlow)
        {
            _containerFlow.CloseContainer();
            parentSequenceContainerFlow.AddContainer(_containerFlow);
            parentSequenceContainerFlow.CloseContainer();
        }

        public T Parallel()
        {
            var parallelContainer = new ParallelContainerFlow();
            _containerFlow.AddContainer(parallelContainer);
            return _referenceFlowApi;
        }

        public T Sequence()
        {
            var sequenceContainer = new SequenceContainerFlow();
            _containerFlow.AddContainer(sequenceContainer);
            return _referenceFlowApi;
        }

        public T Close()
        {
            _containerFlow.CloseContainer();
            return _referenceFlowApi;
        }
    }
}
 