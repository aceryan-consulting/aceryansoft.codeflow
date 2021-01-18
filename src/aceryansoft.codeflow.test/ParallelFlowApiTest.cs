using System.Linq;
using System.Threading;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent; 

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class ParallelFlowApiTest
    {
        [TestMethod]
        public void should_execute_codeflow_with_parallel_container_as_expected()
        {
            var addThreadActivity = new AddThreadIdActivity();
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext() { LastIndex = 2 });
            })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do1|";
                    return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                })
                .Parallel()
                    .Do(AddThreadIdDelegate)
                    .Do(AddThreadIdDelegate)
                    .Do(AddThreadIdDelegate)
                    .Do(addThreadActivity)
                    .If((ctx, inputs) => ((SampleContext)ctx).LastIndex == 2)
                        .Do((ctx, inputs) =>
                        {
                            var ctxData = (SampleContext)ctx;
                            ctxData.ThreadCallStack.Add(Thread.CurrentThread.ManagedThreadId);
                            ctxData.CallStack += "parallel-if-do|";
                            return new model.Config.ExecutionContext() { Status = Status.Succeeded }; // Thread.CurrentThread
                        })
                    .Close()
                .Close()
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do2|";
                    return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            Check.That(contextResult.CallStack).Equals("do1|parallel-if-do|do2|");
            Check.That(contextResult.ThreadCallStack.Distinct().Count()).IsStrictlyGreaterThan(1);
        }

        private IExecutionContext AddThreadIdDelegate(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            Thread.Sleep(100);
            ctxData.ThreadCallStack.Add(Thread.CurrentThread.ManagedThreadId); 
            return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        }
    }

    public class AddThreadIdActivity : ICodeFlowActivity
    {
        public IExecutionContext Execute(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            ctxData.ThreadCallStack.Add(Thread.CurrentThread.ManagedThreadId);
            return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        }
    }
}
