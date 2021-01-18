using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.model.Activities
{
    public interface ICodeFlowActivityCall
    {
        void Execute(ICodeFlowContext context, params object[] inputs);
    }
}