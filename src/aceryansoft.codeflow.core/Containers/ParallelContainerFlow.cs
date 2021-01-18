using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class ParallelContainerFlow : BaseContainerFlow
    { 

        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new ConcurrentBag<IExecutionContext>();

            Parallel.ForEach(PipelineQueue.ToList(), queueDelegate =>
            {
                var innerRes = queueDelegate.Execute(context, activityFilter, inputs);
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