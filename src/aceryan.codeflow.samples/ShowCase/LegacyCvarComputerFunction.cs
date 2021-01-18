using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Activities;

namespace aceryan.codeflow.samples.ShowCase
{
    public class LegacyCvarComputerFunction : ICodeFlowActivityFunction
    {
        public object Execute(ICodeFlowContext context, params object[] inputs)
        {
            return new object();
        }
    }

}
