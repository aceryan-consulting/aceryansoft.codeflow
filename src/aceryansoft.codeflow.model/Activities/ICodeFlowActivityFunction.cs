using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.model.Activities
{
    public interface ICodeFlowActivityFunction
    {
        object Execute(ICodeFlowContext context, params object[] inputs);
    }
}