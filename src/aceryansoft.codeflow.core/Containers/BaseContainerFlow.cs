using System;
using System.Collections.Generic;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class BaseContainerFlow : ActivityFlow ,IContainerFlow
    { 
        protected FlowContainerConfig _config;
        protected Stack<IContainerFlow> InnerContainerStack = new Stack<IContainerFlow>(); 
        protected Queue<IActivityFlow> PipelineQueue = new Queue<IActivityFlow>(); 
        public BaseContainerFlow()
        {
            _config = new FlowContainerConfig();
        }

        public void WithConfig(Action<FlowContainerConfig> configAction)
        {
            configAction?.Invoke(_config);
        }
         
        public void CloseContainer()
        {
            if (InnerContainerStack.Count == 1)
            {
                var innerContainer = InnerContainerStack.Peek();
                if (innerContainer.IsAllContainerClosed())
                {
                    InnerContainerStack.Pop();
                }
                else
                {
                    innerContainer.CloseContainer();
                }
            }
        }

        public bool IsAllContainerClosed()
        {
            return InnerContainerStack.Count == 0;
        }
         

        public void AddActivity(ActivityDelegate activity)
        { 
            if (InnerContainerStack.Count == 0)
            { 
                PipelineQueue.Enqueue(new ActivityFlow() { Activity = activity });
            }
            else if (InnerContainerStack.Count == 1)
            {
                var innerContainer = InnerContainerStack.Peek();
                innerContainer.AddActivity(activity);
            }
        }
        
        public void AddContainer(IContainerFlow container)
        {
            if (InnerContainerStack.Count == 0)
            {
                InnerContainerStack.Push(container);
                PipelineQueue.Enqueue(container);
            }
            else if (InnerContainerStack.Count == 1)
            {
                var innerContainer = InnerContainerStack.Peek();
                innerContainer.AddContainer(container);
            }
        }
         
    }
     
}
 