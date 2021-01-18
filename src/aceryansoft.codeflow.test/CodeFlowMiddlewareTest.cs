using System;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Middlewares;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using Ninject;
using Ninject.Parameters;


namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class CodeFlowMiddlewareTest
    {
        [TestMethod]
        public void should_execute_all_register_middlewares()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(InitMiddlewareWithbeforeAndAfterActions)
            .Call((ctx, inputs) =>
            { 
                ctx.As<SampleContext>().CallStack += "do->"; 
            })
            .Close();

            var contextResult =  codeFlow.Execute();
            Check.That(contextResult.As<SampleContext>().CallStack).Equals("m1.start->m2.start->do->m1.end"); 
        }

        [TestMethod]
        public void should_stop_execution_if_middleware_chain_is_broken()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(InitMiddlewareWithchainBroken)
            .Do((ctx, inputs) =>
            {
                var ctxData = (SampleContext)ctx;
                ctxData.CallStack += "do->";
                return new ExecutionContext() { Status = Status.Succeeded };
            })
            .Close();

            var contextResult =  codeFlow.Execute();
            Check.That(contextResult.As<SampleContext>().CallStack).Equals("m1.start->m2.start->m1.end");
        }

        private void InitMiddlewareWithbeforeAndAfterActions(ICodeFlowExecutionConfig configAction)
        {
            configAction.WithContext(() => new SampleContext())
                .UseMiddleware((ctx, next) =>
                {
                    var ctxData = ctx.As<SampleContext>();
                    ctxData.CallStack += "m1.start->";
                    next();
                    ctxData.CallStack += "m1.end";
                }).UseMiddleware((ctx, next) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "m2.start->";
                    next();
                });
        }

        private void InitMiddlewareWithchainBroken(ICodeFlowExecutionConfig configAction)
        {
            configAction.WithContext(() => new SampleContext())
                .UseMiddleware((ctx, next) =>
                {
                    var ctxData = ctx.As<SampleContext>();
                    ctxData.CallStack += "m1.start->";
                    next();
                    ctxData.CallStack += "m1.end";
                }).UseMiddleware((ctx, next) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "m2.start->";
                    // next(); // break execution chain 
                });
        }

        [TestMethod]
        public void should_execute_middleware_instance_with_dependency_injection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICodeFlowMiddleWare>().ToConstructor(x => new SampleMiddleware(2)); 


            var sampleMiddleware = new SampleMiddleware(1);
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .WithServiceResolver((typ, str) => kernel.Get<ICodeFlowMiddleWare>()) 
                    .UseMiddleware((ctx, next) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "m1.start->";
                        next();
                        ctxData.CallStack += "m1.end";
                    })
                    .UseMiddleware(sampleMiddleware)
                    .UseMiddleware<SampleMiddleware>();
            })
            .Do((ctx, inputs) =>
            {
                var ctxData = (SampleContext)ctx;
                ctxData.CallStack += "do->";
                return new ExecutionContext() { Status = Status.Succeeded };
            })
            .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("m1.start->middleware-mock-1->middleware-mock-2->do->m1.end");
        }

        [TestMethod]
        public void should_execute_error_middleware_and_stop_pipeline()
        {
            var sampleMiddleware = new SampleMiddleware(1);
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext())
                    .UseMiddleware((ctx, next) =>
                    {
                        var ctxData = (SampleContext)ctx;
                        ctxData.CallStack += "m1.start->";
                        next();
                        ctxData.CallStack += "m1.end";
                    })
                    .UseMiddleware((ctx, next)=>
                    {
                        try
                        {
                            next();
                        }
#pragma warning disable 168
                        catch (Exception e)
#pragma warning restore 168
                        {
                            ctx.As<SampleContext>().CallStack += "middleware-catch-and-stop->";
                        }
                    })
                    .UseMiddleware(sampleMiddleware);
            })
            .Call((ctx, inputs) =>
            {
                throw new Exception("some error");
            })
            .Do((ctx, inputs) =>
            {
                var ctxData = (SampleContext)ctx;
                ctxData.CallStack += "do->";
                return new ExecutionContext() { Status = Status.Succeeded };
            })
            .Close();

            var contextResult = (SampleContext)codeFlow.Execute();

            Check.That(contextResult.CallStack).Equals("m1.start->middleware-mock-1->middleware-catch-and-stop->m1.end");
        }


    }
}
