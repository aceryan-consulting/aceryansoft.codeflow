using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aceryan.codeflow.samples.GettingStarted
{
    public class ForeachCodeFlow
    {
        public void Run()
        { 
            var foreachcodeFlow = new CodeFlow();
            foreachcodeFlow.StartNew(cfg => cfg.WithContext(() => new ForeachContext()))
            .ForEach((ctx, inputs) => Enumerable.Range(0,2).OfType<object>().ToList() ) 
                .AsSequence()
                   .ForEach((ctx, inputs) => Enumerable.Range(1,1).OfType<object>().ToList())
                        .AsParallel()
                             .Function((ctx, inputs) =>
                             {
                                 // for the first iteration inputs[0]=0 , inputs[1]=1 index on the inner iteration, inputs[2]= 7 SomeArg value
                                 return (int)inputs[0] + (int)inputs[1] + (int)inputs[2];
                             }).WithArg<int>("SomeArg", (ctx, inputs) => 7)
                             .WithResult<int>((ctx, inputs, res) => ctx.As<ForeachContext>().Results.Add(res))
                        .Close()
                    .CloseForEach()
                .Close()
            .CloseForEach() 
            .Close();

            foreachcodeFlow.Execute();
        }         
    }

    public class ForeachContext : CodeFlowContext
    {
        public ConcurrentBag<int> Results { get; set; } = new ConcurrentBag<int>();

        public ForeachContext()
        {
            ContextProperties["Results"] = new ContextProperty(() => Results, (obj) => { Results = (ConcurrentBag<int>)obj; });
        }
    }

}
