using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.test.TestModel
{
    public class SampleCallActivity : ICodeFlowActivityCall
    {
        private readonly string _name;

        public SampleCallActivity(string name)
        {
            _name = name;
        }
        public void Execute(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            ctxData.CallStack += $"call-{_name}->";
        }
    }
}