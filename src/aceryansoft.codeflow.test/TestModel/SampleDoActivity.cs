using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.test.TestModel
{
    public class SampleDoActivity : ICodeFlowActivity
    {
        private readonly string _name;

        public SampleDoActivity(string name)
        {
            _name = name;
        }
        public IExecutionContext Execute(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            ctxData.CallStack += $"do-{_name}->";
            return new ExecutionContext() { Status = Status.Succeeded };
        }
    }
}