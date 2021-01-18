using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using Ninject;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class DoCallFlowApiTest
    {
        [TestMethod]
        public void should_execute_do_call_operator_with_delegate_instance_and_dependency_injection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICodeFlowActivity>().ToConstructor(x => new SampleDoActivity("i1")).Named("instance_di1");
            kernel.Bind<ICodeFlowActivityCall>().ToConstructor(x => new SampleCallActivity("c1")).Named("call_di2");

            var doinstance2 = new SampleDoActivity("i2");
            var callinstance2 = new SampleCallActivity("c2");
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext())
                        .WithServiceResolver((typ, str) => string.IsNullOrEmpty(str) ? kernel.Get(typ) : kernel.Get(typ,str));  
                })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do0->";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Do(doinstance2)
                .Do<ICodeFlowActivity>("instance_di1")
                .Call((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "call0->"; 
                })
                .Call(callinstance2)
                .Call<ICodeFlowActivityCall>("call_di2")
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            Check.That(contextResult.CallStack).Equals("do0->do-i2->do-i1->call0->call-c2->call-c1->");
        }


        [TestMethod]
        public void should_execute_do_call_operator_with_call_block_delegate_instance_and_dependency_injection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICodeFlowActivity>().ToConstructor(x => new SampleDoActivity("i0"));

            var instance1 = new SampleDoActivity("i1");
            var instance2 = new SampleDoActivity("i_unknown");
            var instance3 = new SampleDoActivity("i3");
            var callinstance2 = new SampleCallActivity("c2");

            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .WithServiceResolver((typ, str) => string.IsNullOrEmpty(str) ? kernel.Get(typ) : kernel.Get(typ, str));
            })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do0->";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Do(instance1)
                .Do<ICodeFlowActivity>()
                .CallCodeFlow(callblock =>
                {
                    callblock
                       .Do((ctx, inputs) =>
                       {
                           var ctxData = (SampleContext)ctx;
                           ctxData.CallStack += "do-call-block->";
                           return new ExecutionContext() { Status = Status.Succeeded };
                       })
                       .If((ctx, inputs) => ctx.Host == "unknown")
                           .Do(instance2)
                       .Close()
                       .Call(callinstance2)
                       .Do(instance3);
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("do0->do-i1->do-i0->do-call-block->call-c2->do-i3->");
        }



    }
}
