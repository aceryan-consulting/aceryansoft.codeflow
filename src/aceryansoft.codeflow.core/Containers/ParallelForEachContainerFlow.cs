using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class ParallelForEachContainerFlow : IteratorContainerFlow
    { 
        public ParallelForEachContainerFlow(Func<ICodeFlowContext, object[], List<object>> itemsIterator, int packetSize = 0)
        : base(itemsIterator, packetSize)
        { 
        }

        protected override IExecutionContext ExecuteOnItems(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, List<object> items, params object[] inputs)
        {
            var childResults = new ConcurrentBag<IExecutionContext>();
            var defaultInputs = inputs != null && inputs.Any() ? inputs.ToList() : new List<object>();
            var concurrentList = new ConcurrentBag<object>(defaultInputs);
            Parallel.ForEach(items, item =>
            {
                var eachInput = concurrentList.ToList();
                eachInput.Add(item);

                var innerRes = ExecuteForEach(context, activityFilter, eachInput.ToArray());
                childResults.Add(innerRes);
            });
            var childResultList = childResults.ToList();
            int lastErrorCode = 0;
            if (childResultList.Any(x => x.Status == Status.Failed))
            {
                var lastFailed = childResultList.Last(x => x.Status == Status.Failed);
                lastErrorCode = lastFailed.ErrorCode;
            }
            return ExecutionContext.GetCombinedSequenceResult(childResultList, lastErrorCode);
        } 

    }
}
