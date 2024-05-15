## *** Release note : version 1.24.05.15 ***
###  What's new 
update interface IFlowApi to enabled running parralel sequence container 
(Warning NETSDK1138	: The target framework 'netcoreapp3.1' is out of support,  Please refer to https://aka.ms/dotnet-core-support)

```c# 
  public interface IFlowApi<T> : ILoopFlowApi<T,T>
    {
        T CallCodeFlow(Action<ICallFlowApi> codeFlowBlock);
        T Parallel();
        T Sequence(); // expose new sequence container
        T Close();
    }
``` 
sample code 

```c# 
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
                                return new model.Config.ExecutionContext() { Status = Status.Succeeded }; 
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
            
``` 

## *** Release note : version 1.21.01.18 ***
###  What's new 
simple and fluent api 

middleware pipeline (error)

activity filter pipeline (logs,perfs)

activity flow executers (Do,Call)

sequence flow containers (if, while)

parallel flow containers (parallel)

switch flow container (switch, case, default) 

iterator flow container (ForEach), with sequential and parallel execution

function flow container (Func) with inputs and result

reusable codeflow (CallCodeFlow)

RestorePoint to enhance codeflow rerun and state saving

multi target package (dotnet standard 2.0 and dotnet framework 4.5.2)


### Bug fixes 



## *** Release note : version 1.21.04.19 ***
### What's new 
Codeflow context use reflection to ease properties setup 

Codeflow context automatically cast context to itself or any base class

```c# 
public class AiContext : CodeFlowContext
    { 
        public decimal LoyaltyThreshold { get; set; } 

        public AiContext()
        { 
		    // *** no more need to define properties setters
            //ContextProperties["LoyaltyThreshold"] = new ContextProperty(() => LoyaltyThreshold, (obj) => { LoyaltyThreshold = (decimal)obj; }); 
        }
		
        //public override T As<T>()   // *** no more need to override context cast method by default
        //{ 
        //}
    }
``` 

### Bug fixes 



 
 

 
