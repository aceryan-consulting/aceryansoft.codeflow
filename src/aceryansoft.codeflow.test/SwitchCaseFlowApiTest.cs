using System.Linq;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class SwitchCaseFlowApiTest
    {
        [TestMethod]
        public void should_execute_switch_case_on_block_and_foreach_sequence_container()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext(){Host = "localhost"}) ;
                })
                .Switch((ctx, inputs)=> ctx.Host)
                    .Case((obj, inputs) => (string) obj=="localhost")
                       .Call((ctx,inputs) => { ctx.As<SampleContext>().CallStack += "case-call-match->"; })
                    .Case((obj, inputs) => (string) obj != "localhost")
                        .Call((ctx, inputs) => { ctx.As<SampleContext>().CallStack += "case-call-no-match->"; })
                .CloseSwitch()
                .ForEach((obj, inputs) => Enumerable.Range(1, 3).OfType<object>().ToList())
                    .AsSequence()
                        .Switch((ctx, inputs) => inputs[0])
                            .Case((obj, inputs) => (int)obj == 1)
                              .Call((ctx, inputs) => { ctx.As<SampleContext>().CallStack += $"each-case-{inputs[0]}->"; })
                            .Case((obj, inputs) => (int)obj == 2)
                                .Call((ctx, inputs) => { ctx.As<SampleContext>().CallStack += $"each-case-{inputs[0]}->"; })
                        .Default()
                            .Call((ctx, inputs) => { ctx.As<SampleContext>().CallStack += $"each-default-{inputs[0]}->"; })
                        .CloseSwitch()
                    .Close()
                .CloseForEach()
                    .Call((ctx, inputs) => { ctx.As<SampleContext>().CallStack += $"do-inputs[0]={inputs?.Length}"; }) // 0 expected because not in each block
                .Close(); 
            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("case-call-match->each-case-1->each-case-2->each-default-3->do-inputs[0]=0");
        }
    }
}