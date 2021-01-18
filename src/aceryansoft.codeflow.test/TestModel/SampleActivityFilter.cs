using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Filter;

namespace aceryansoft.codeflow.test.TestModel
{
    public class SampleActivityFilter : ICodeFlowActivityFilter
    {
        public void OnActivityExecuting(ICodeFlowContext context, params object[] inputs)
        {
            context.As<SampleContext>().CallStack += $"class-filter-before->";
        }

        public void OnActivityExecuted(ICodeFlowContext context, IExecutionLog executionLog, params object[] inputs)
        {
            context.As<SampleContext>().CallStack += $"class-filter-after-{executionLog.ActivityName}->";
        }
    }
}