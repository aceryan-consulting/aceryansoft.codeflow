using System;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.model.Containers
{
    public interface IContainerFlow : IActivityFlow
    {
        void WithConfig(Action<FlowContainerConfig> configAction);  
        void CloseContainer();
        bool IsAllContainerClosed(); 
        void AddActivity(ActivityDelegate activity);
        void AddContainer(IContainerFlow container);
   }
}