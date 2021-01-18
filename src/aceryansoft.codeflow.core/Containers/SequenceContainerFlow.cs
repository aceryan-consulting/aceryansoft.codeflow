using System;
using System.Collections.Generic;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class SequenceContainerFlow : BaseContainerFlow
    {
        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0;
            foreach (var pipeline in PipelineQueue)
            {
                var actResult = pipeline.Execute(context, activityFilter, inputs);
                childResults.Add(actResult);
                if (actResult.Status == Status.Failed && !_config.ContinueOnError)
                {
                    lastErrorCode = actResult.ErrorCode;
                    break;
                }
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode);
        }
    }
}
