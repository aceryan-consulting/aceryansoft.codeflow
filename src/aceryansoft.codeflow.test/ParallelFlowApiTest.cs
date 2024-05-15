using System.Collections.Generic;
using System.Linq;
using System;
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

        [TestMethod]
        public void should_execute_parallel_container_containing_sequence_as_expected()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext() { LastIndex = 2 });
            })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "seq-start|";
                    return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                })
                .Parallel()
                    .Do((ctx, inputs) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        var threadId1 = Thread.CurrentThread.ManagedThreadId;
                        ctxData.CallStack += $"do-para-1-{threadId1}|";
                        ctxData.ThreadCallStack.Add(threadId1);
                        return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                    })
                    .Sequence()
                       .Do((ctx, inputs) =>
                        {
                            var ctxData = (SampleContext)ctx;
                            var threadId2 = Thread.CurrentThread.ManagedThreadId;
                            ctxData.ThreadCallStack.Add(threadId2);
                            ctxData.CallStack += $"seq-para-2-{threadId2}|";
                            return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                        })
                        .If((ctx, inputs) => ((SampleContext)ctx).LastIndex == 2)
                            .Do((ctx, inputs) =>
                            {
                                var ctxData = (SampleContext)ctx; 
                                ctxData.CallStack += "seq-para-if-3|";
                                return new model.Config.ExecutionContext() { Status = Status.Succeeded }; // Thread.CurrentThread
                            })
                        .Close()
                    .Close()
                .Close()
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "seq-end|";
                    return new model.Config.ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            
            Check.That(contextResult.CallStack.StartsWith("seq-start|")).IsTrue();
            Check.That(contextResult.CallStack.EndsWith("seq-end|")).IsTrue();
            Check.That(contextResult.ThreadCallStack.Distinct().Count()).IsEqualTo(2);
            var threadIds = contextResult.ThreadCallStack.ToArray();
            var possibleResults = new List<string>()
            {
                $"do-para-1-{threadIds[0]}|seq-para-2-{threadIds[1]}|seq-para-if-3|", $"do-para-1-{threadIds[1]}|seq-para-2-{threadIds[0]}|seq-para-if-3|",
                $"seq-para-2-{threadIds[0]}|seq-para-if-3|do-para-1-{threadIds[1]}|", $"seq-para-2-{threadIds[1]}|seq-para-if-3|do-para-1-{threadIds[0]}|",
            };
            var parrallelCallStack = contextResult.CallStack.Replace("seq-start|", string.Empty).Replace("seq-end|", String.Empty);
            Check.That(possibleResults.Contains(parrallelCallStack)).IsTrue();
        }

        //[TestMethod]
        //public void should_fail_parallel_container_containing_sequence_not_closed()
        //{
        //    var codeFlow = new CodeFlow();
        //    var codeFlowApi = codeFlow.StartNew(cfg =>
        //    {
        //        cfg.WithContext(() => new SampleContext() { LastIndex = 2 });
        //    })
        //        .Do((ctx, inputs) =>
        //        {
        //            var ctxData = (SampleContext)ctx;
        //            ctxData.CallStack += "seq-start|";
        //            return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        //        })
        //        .Parallel()
        //            .Do((ctx, inputs) =>
        //            {
        //                var ctxData = (SampleContext)ctx;
        //                var threadId1 = Thread.CurrentThread.ManagedThreadId;
        //                ctxData.CallStack += $"do-para-1-{threadId1}|";
        //                ctxData.ThreadCallStack.Add(threadId1);
        //                return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        //            })
        //            .Sequence()
        //               .Do((ctx, inputs) =>
        //               {
        //                   var ctxData = (SampleContext)ctx;
        //                   var threadId2 = Thread.CurrentThread.ManagedThreadId;
        //                   ctxData.ThreadCallStack.Add(threadId2);
        //                   ctxData.CallStack += $"seq-para-2-{threadId2}|";
        //                   return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        //               })
        //                .If((ctx, inputs) => ((SampleContext)ctx).LastIndex == 2)
        //                    .Do((ctx, inputs) =>
        //                    {
        //                        var ctxData = (SampleContext)ctx;
        //                        ctxData.CallStack += "seq-para-if-3|";
        //                        return new model.Config.ExecutionContext() { Status = Status.Succeeded }; // Thread.CurrentThread
        //                    })
        //                //.Close()
        //        //.Close() ignore sequence closing, this cause the last sequance to run inside the Parallel() container 
        //        //todo yannick add a feature to detect this case before execution of the codeflow
        //        .Close()
        //        .Do((ctx, inputs) =>
        //        {
        //            var ctxData = (SampleContext)ctx;
        //            ctxData.CallStack += "seq-end|"; 
        //            return new model.Config.ExecutionContext() { Status = Status.Succeeded };
        //        })
        //        .Close();

        //    Check.That(codeFlowApi.IsAllContainerClosed()).IsFalse();
        //}

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
