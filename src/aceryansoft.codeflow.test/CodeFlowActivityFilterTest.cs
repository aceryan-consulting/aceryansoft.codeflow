using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class CodeFlowActivityFilterTest
    {
        [TestMethod]
        public void should_run_registered_activity_filters_on_each_activities()
        {
            var codeFlow = new CodeFlow();
            var sampleActivityFilter = new SampleActivityFilter();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext() { Host = "local-host" })
                     .UseActivityFilter((ctx, inputs, next) =>
                     {
                         var res = next(ctx, inputs);
                         ctx.As<SampleContext>().CallStack += $"filter-2-{res.ActivityName}->";
                         return res;
                     })
                    .UseMiddleware((ctx, next) =>
                    {
                        ctx.As<SampleContext>().CallStack += "m1.start->";
                        next();
                        ctx.As<SampleContext>().CallStack += "m1.end";
                    })
                   .UseActivityFilter(sampleActivityFilter);
                    })
                    .Do((ctx, inputs) =>
                    {
                        ctx.As<SampleContext>().CallStack += "do->";
                        return new ExecutionContext() { Status = Status.Succeeded, ActivityName = "do-activity" };
                    })
                    .CallCodeFlow(callblock =>
                    {
                        callblock
                           .Do((ctx, inputs) =>
                           {
                               ctx.As<SampleContext>().CallStack += "do-call-block->";
                               return new ExecutionContext() { Status = Status.Succeeded, ActivityName = "do-call-activity" };
                           })
                           .If((ctx, inputs) => ctx.Host == "local-host")
                               .Do((ctx, inputs) =>
                               {
                                   ctx.As<SampleContext>().CallStack += "if->";
                                   return new ExecutionContext() { Status = Status.Succeeded, ActivityName = "if-activity" };
                               })
                           .Close();
                    })
            .Close();

            var contextResult = codeFlow.Execute();

            Check.That(contextResult.As<SampleContext>().CallStack)
            .Equals("m1.start->class-filter-before->do->class-filter-after-do-activity->filter-2-do-activity->class-filter-before->do-call-block->class-filter-after-do-call-activity->filter-2-do-call-activity->class-filter-before->if->class-filter-after-if-activity->filter-2-if-activity->m1.end");
        }

        [TestMethod]
        public void should_use_activity_filters_to_rerun_some_activity()
        {
            var codeFlow = new CodeFlow(); 
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext() {Host = "local-host"})
                        .UseActivityFilter((ctx, inputs, next) =>
                        {
                            var res = next(ctx, inputs);
                            ctx.As<SampleContext>().CallStack += $"filter-{res.ActivityName}->";
                            if (res.ActivityName == "do-call-activity") // rerun do-call-activity
                            {
                                next(ctx, inputs);
                            }
                            return res;
                        });
                })
                    .Do((ctx, inputs) =>
                    {
                        ctx.As<SampleContext>().CallStack += "do->";
                        return new ExecutionContext() { Status = Status.Succeeded, ActivityName = "do-activity" };
                    })
                    .CallCodeFlow(callblock =>
                    {
                        callblock
                           .Do((ctx, inputs) =>
                           {
                               ctx.As<SampleContext>().CallStack += "do-call-block->";
                               return new ExecutionContext() { Status = Status.Succeeded, ActivityName = "do-call-activity" };
                           });
                    })
            .Close();

            var contextResult = codeFlow.Execute();

            Check.That(contextResult.As<SampleContext>().CallStack)
            .Equals("do->filter-do-activity->do-call-block->filter-do-call-activity->do-call-block->");
        }
    }
}
