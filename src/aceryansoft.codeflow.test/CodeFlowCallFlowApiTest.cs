using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.FlowApi;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class CodeFlowCallFlowApiTest
    {

        [TestMethod]
        public void should_call_and_execute_inner_codeflow_container()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext());
                })
                .Do((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "do1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .CallCodeFlow(GetInnerCodeFlow)
                .Do((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "do2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = codeFlow.Execute();
            Check.That(contextResult.As<SampleContext>().CallStack).Equals("do1|call-init|call-If|call-sub-do|do2|");
        }

        private void GetInnerCodeFlow(ICallFlowApi blockPath)
        {
            blockPath
                .Do((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "call-init|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .If((ctx, inputs) => ((SampleContext)ctx).CallStack.Contains("call-init|"))
                .Do((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "call-If|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close()
                .CallCodeFlow(GetSubInnerCodeFlow);
        }

        private void GetSubInnerCodeFlow(ICallFlowApi blockPath)
        {
            blockPath
                .Do((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "call-sub-do|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();
        }

    }
}