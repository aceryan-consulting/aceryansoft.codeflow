using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.model.FlowApi
{ 

    public interface IForEachFlowApi<T>
    {
        IForEachInnerFlowApi<T> AsSequence(Action<FlowContainerConfig> configAction = null);
        IForEachInnerFlowApi<T> AsParallel();
    }

    public interface IForEachInnerFlowApi<T> : ILoopFlowApi<ICloseForEachInnerFlowApi<T>, IForEachInnerFlowApi<T>>//ICommonFlowApi<ICloseForEachInnerFlowApi<T>>
    { 
        ICloseForEachInnerFlowApi<T> Close();  
    }

    public interface ICloseForEachInnerFlowApi<T> : IForEachInnerFlowApi<T>
    {
        T CloseForEach();
    }
    
}