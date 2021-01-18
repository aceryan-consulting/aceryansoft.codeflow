using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using ExecutionContext = aceryansoft.codeflow.model.Config.ExecutionContext;

namespace aceryansoft.codeflow.test
{
    // todo add test on foreach with packet size 
    [TestClass]
    public class ForeachFlowApiTest
    {
        [TestMethod]
        public void should_execute_all_operations_on_sequence_container_with_inner_each_containers()
        {
            var codeFlow = new CodeFlow();
            Action<ICodeFlowExecutionConfig> configInit = cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .UseMiddleware((ctx, next) =>
                    {
                        var ctxData = ctx.As<SampleContext>();
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
                .ForEach((ctx, inputs) => Enumerable.Range(2, 2).OfType<object>().ToList())
                .AsSequence()
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += $"on_item-{inputs[0]}|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .If((ctx, item) => (int)item[0] % 2 == 0)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += $"even-number-{inputs[0]}|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close()
                .While((ctx, item) => (int)item[0] == 3 && ((SampleContext)ctx).LastIndex != -1)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += $"while-{inputs[0]}|";
                    ctxData.LastIndex = -1;
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close()
                .CloseForEach();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("m1.start|do-init|on_item-2|even-number-2|on_item-3|while-3|m1.end");
        }


        [TestMethod]
        public void should_execute_all_operations_on_sequence_container_with_inner_parallel_containers()
        {
            var codeFlow = new CodeFlow();
            Action<ICodeFlowExecutionConfig> configInit = cfg =>
            {
                cfg.WithContext(() => new SampleContext());
            };

            codeFlow.StartNew(configInit)
                .ForEach((ctx, inputs) => Enumerable.Range(2, 5).OfType<object>().ToList())
                .AsParallel()
                .Do(AddThreadAndItemDelegate)
                .CloseForEach();

            var contextResult = (SampleContext)codeFlow.Execute();
            var parallelCallStack = contextResult.ConcurrentCallStack.ToList().OrderBy(x => x).ToList();
            Check.That(parallelCallStack).Equals(new List<string>() { "2", "3", "4", "5", "6" });
            Check.That(contextResult.ThreadCallStack.Distinct().Count()).IsStrictlyGreaterThan(1);
        }

        private IExecutionContext AddThreadAndItemDelegate(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            Thread.Sleep(100);
            ctxData.ThreadCallStack.Add(Thread.CurrentThread.ManagedThreadId);
            ctxData.ConcurrentCallStack.Add(inputs[0]?.ToString());

            return new ExecutionContext() { Status = Status.Succeeded };
        }

        [TestMethod]
        public void should_check_parameter_transmission_between_inner_foreach_block()
        {
            var codeFlow = new CodeFlow();
            Action<ICodeFlowExecutionConfig> configInit = cfg =>
            {
                cfg.WithContext(() => new SampleContext());
            };

            codeFlow.StartNew(configInit)
                .Call((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "call-init|";
                })
                .ForEach((ctx, inputs) => Enumerable.Range(2, 2).OfType<object>().ToList())
                .AsSequence()
                .Call((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += $"call-{inputs[0]}|";

                })
                .Function(AddFunctionLevel1)
                .WithArg<int>("", (ctx, inputs) => (int)inputs[0])
                .WithResult<int>((ctx, inputs, res) =>
                {
                    ctx.As<SampleContext>().CallStack += $"result={res}|";
                })
                .Close()
                .If((ctx, item) => (int)item[0] % 2 == 0)
                .Call((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += $"even-number-{inputs[0]}|";
                })
                .Close()
                .While((ctx, item) => (int)item[0] == 3 && ((SampleContext)ctx).LastIndex != -1)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += $"while-{inputs[0]}|";
                    ctxData.LastIndex = -1;
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close()
                .Switch((ctx, item) => (int)item[0] % 2 == 0)
                .Case((val, item) => (bool)val)
                .Call((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += $"even-number-{inputs[0]}|";
                })
                .Default()
                .Call((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += $"odd-number-{inputs[0]}|";
                })
                .CloseSwitch().Close()
                .CloseForEach()
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("call-init|call-2|result=4|even-number-2|even-number-2|call-3|result=6|while-3|odd-number-3|");
        }

        private object AddFunctionLevel1(ICodeFlowContext context, params object[] inputs)
        {
            return (int)inputs[0] + (int)inputs[1];
        }


        [TestMethod]
        public void should_call_foreach_inside_foreach_block_and_check_inputs_parameters_sharing()
        {
            var codeFlow = new CodeFlow();
            Action<ICodeFlowExecutionConfig> configInit = cfg =>
            {
                cfg.WithContext(() => new SampleContext());
            };

            codeFlow.StartNew(configInit)
                .Call((ctx, inputs) =>
                {
                    ctx.As<SampleContext>().CallStack += "call-init|";
                })
                .ForEach((ctx, inputs) => Enumerable.Range(1, 2).OfType<object>().ToList())
                    .AsSequence()
                        .ForEach((ctx, inputs) => Enumerable.Range(1, 2).OfType<object>().ToList())
                            .AsSequence()
                            .Call((ctx, inputs) =>
                            {
                                var ctxData = (SampleContext)ctx;
                                ctxData.CallStack += $"call-{inputs[0]}-{inputs[1]}|";
                            })
                            .Function(AddFunctionLevel1)
                                .WithArg<int>("", (ctx, inputs) => (int)inputs[0])
                                .WithResult<int>((ctx, inputs, res) =>
                                {
                                    ctx.As<SampleContext>().CallStack += $"result={res}|";
                                })
                            .Close()
                    .Close()
                .CloseForEach()
                .Close()
                .CloseForEach()
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("call-init|call-1-1|result=2|call-1-2|result=3|call-2-1|result=3|call-2-2|result=4|");
        }
    }
}