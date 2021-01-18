using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;
using System;

namespace aceryansoft.codeflow.model.Containers
{
    public interface IActivityFlow
    {
        ActivityDelegate Activity { get; set; } 
        IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs);
    }

    public class ActivityFlow : IActivityFlow
    {
        public ActivityDelegate Activity { get; set; }
        
        public virtual IExecutionContext Execute(ICodeFlowContext context,Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            if (activityFilter != null)
            {
               return activityFilter(Activity)?.Invoke(context, inputs);
            }
            return Activity?.Invoke(context, inputs) ?? ExecutionContext.GetDefault();
        }
    }
}
