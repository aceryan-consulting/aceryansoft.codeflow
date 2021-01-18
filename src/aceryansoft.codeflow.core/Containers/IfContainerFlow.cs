using System;
using System.Collections.Generic;
using System.Linq;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class IfContainerFlow : SequenceContainerFlow
    {
        private readonly Func<ICodeFlowContext,object[], bool> _ifSelector;

        public IfContainerFlow(Func<ICodeFlowContext, object[], bool> ifSelector)
        {
            _ifSelector = ifSelector;
        }
         
        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0; 
            if (_ifSelector(context, inputs))
            {
                return base.Execute(context, activityFilter, inputs);
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode);
        }
    }
}
 