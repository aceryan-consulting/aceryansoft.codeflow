using System;
using System.Collections.Generic;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class WhileContainerFlow : SequenceContainerFlow
    {
        private readonly Func<ICodeFlowContext, object[], bool> _whileSelector;

        public WhileContainerFlow(Func<ICodeFlowContext, object[], bool> whileSelector)
        {
            _whileSelector = whileSelector;
        }
         
        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0; 

            while (_whileSelector(context, inputs))
            {
                var actResult = base.Execute(context, activityFilter, inputs);
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