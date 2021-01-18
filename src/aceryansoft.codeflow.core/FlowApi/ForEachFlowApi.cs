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
    internal class ForEachFlowApi<T> : LoopFlowApi<ICloseForEachInnerFlowApi<T>, IForEachInnerFlowApi<T>>, IForEachFlowApi<T>, ICloseForEachInnerFlowApi<T>
    {
        private readonly T _parentBlockPath;
        private readonly IContainerFlow _parentcontainerFlow;
        private readonly Func<ICodeFlowContext, object[], List<object>> _iterator;
        private readonly int _packetSize;

        internal ForEachFlowApi(T parentBlockPath, IContainerFlow parentcontainerFlow
            , Func<ICodeFlowContext, object[], List<object>> iterator, CodeFlowExecutionConfig config, int packetSize = 0) :base(null, config)
        {
            _parentBlockPath = parentBlockPath;
            _parentcontainerFlow = parentcontainerFlow;
            _iterator = iterator;
            _packetSize = packetSize;
            SetReferenceFlowApiWithResult(this);
        }

        public IForEachInnerFlowApi<T> AsSequence(Action<FlowContainerConfig> configAction = null)
        {
            _containerFlow = new ForEachContainerFlow(_iterator, _packetSize);
            _parentcontainerFlow.AddContainer(_containerFlow);
            return this;
        }

        public IForEachInnerFlowApi<T> AsParallel()
        {
            _containerFlow = new ParallelForEachContainerFlow(_iterator, _packetSize);
            _parentcontainerFlow.AddContainer(_containerFlow);
            return this;
        }
        
        public ICloseForEachInnerFlowApi<T> Close()
        {
            _containerFlow.CloseContainer();
            return this;
        }
         
        public T CloseForEach()
        {
            if (!_containerFlow.IsAllContainerClosed()) // close previous case or default statements
            {
                _containerFlow.CloseContainer();
            }
            _parentcontainerFlow.CloseContainer(); // close for each container block on parent 
            return _parentBlockPath;
        }
    }
}
