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



 
 

 
