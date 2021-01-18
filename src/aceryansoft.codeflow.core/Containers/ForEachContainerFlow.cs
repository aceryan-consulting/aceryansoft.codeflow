using System;
using System.Collections.Generic;
using System.Linq;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.core.Helpers; 


namespace aceryansoft.codeflow.core.Containers
{
    internal class ForEachContainerFlow : IteratorContainerFlow
    { 
        public ForEachContainerFlow(Func<ICodeFlowContext, object[], List<object>> itemsIterator, int packetSize=0)
            :base(itemsIterator,packetSize)
        { 
        }

        protected override IExecutionContext ExecuteOnItems(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, List<object> items, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0;
            foreach (var item in items)
            {
                var eachInput = inputs != null && inputs.Any() ? inputs.ToList() : new List<object>();
                eachInput.Add(item);
                var innerRes = ExecuteForEach(context, activityFilter, eachInput.ToArray());
                childResults.Add(innerRes);
                if (innerRes.Status == Status.Failed && !_config.ContinueOnError)
                {
                    lastErrorCode = innerRes.ErrorCode;
                    break;
                }
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode);
        }      
    }
}
