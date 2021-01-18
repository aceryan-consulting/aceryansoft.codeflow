using System;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class IfWhileFlowApiTest
    { 
        [TestMethod]
        public void should_execute_all_operations_on_sequence_container()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .UseMiddleware((ctx, next) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "m1.start|";
                        next();
                        ctxData.CallStack += "m1.end";
                    });
            })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                }).Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do3|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("m1.start|do1|do2|do3|m1.end");
        }

        [TestMethod]
        public void should_execute_all_operations_on_sequence_if_while_container_with_inner_sequence_containers()
        {
            var codeFlow = new CodeFlow();
            int whileIndex = 3;
            Action<ICodeFlowExecutionConfig> configInit = cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .UseMiddleware((ctx, next) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "m1.start|";
                        next();
                        ctxData.CallStack += "m1.end";
                    });
            };

            codeFlow.StartNew(configInit)
            .Do((ctx, inputs) =>
            {
                var ctxData = (SampleContext)ctx;
                ctxData.CallStack += "do-init|";
                return new ExecutionContext() { Status = Status.Succeeded };
            })
            .If((ctx, inputs) => ((SampleContext)ctx).CallStack.Contains("do-init|"))
                    .Do((ctx, inputs) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "do-If|";
                        return new ExecutionContext() { Status = Status.Succeeded };
                    })
            .Close()
            .If((ctx, inputs) => ((SampleContext)ctx).CallStack.Contains("unknown-str"))
                    .Do((ctx, inputs) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "do-unknown|";
                        return new ExecutionContext() { Status = Status.Succeeded };
                    })
            .Close()
            .While((ctx, inputs) => whileIndex > 0)
                    .Do((ctx, inputs) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += $"do-while-{whileIndex}|";
                        ctxData.LastIndex = whileIndex;
                        whileIndex--;
                        return new ExecutionContext() { Status = Status.Succeeded };
                    })
                    .Switch((ctx, inputs) => ((SampleContext)ctx).LastIndex)
                        .Case((idxValue, inputs) => (int)idxValue == 3)
                            .Do((ctx, inputs) =>
                            {
                                var ctxData = (SampleContext)ctx;
                                ctxData.CallStack += $"do-case-{ctxData.LastIndex}|";
                                return new ExecutionContext() { Status = Status.Succeeded };
                            })
                        .Case((idxValue, inputs) => (int)idxValue == 9)
                            .Do((ctx, inputs) =>
                            {
                                var ctxData = (SampleContext)ctx;
                                ctxData.CallStack += $"do-case-{ctxData.LastIndex}|";
                                return new ExecutionContext() { Status = Status.Succeeded };
                            })
                        .Default()
                            .Do((ctx, inputs) =>
                            {
                                var ctxData = (SampleContext)ctx;
                                ctxData.CallStack += $"do-default-{ctxData.LastIndex}|";
                                return new ExecutionContext() { Status = Status.Succeeded };
                            })
                    .CloseSwitch()
            .Close()
            .Do((ctx, inputs) =>
            {
                var ctxData = (SampleContext)ctx;
                ctxData.CallStack += $"do-main-sequence|";
                whileIndex--;
                return new ExecutionContext() { Status = Status.Succeeded };
            })
            .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("m1.start|do-init|do-If|do-while-3|do-case-3|do-while-2|do-default-2|do-while-1|do-default-1|do-main-sequence|m1.end");
        }

    }
}