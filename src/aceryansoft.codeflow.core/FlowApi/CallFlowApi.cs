using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class CallFlowApi : BaseFlowApi<ICallFlowApi>, ICallFlowApi
    {
        internal CallFlowApi(SequenceContainerFlow sequenceContainerFlow, CodeFlowExecutionConfig config)
            : base(sequenceContainerFlow, config)
        {
            SetReferenceFlowApiWithResult(this);
        }
        

    }
}