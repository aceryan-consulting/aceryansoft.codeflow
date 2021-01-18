using System;
using System.Collections.Generic;
using System.Linq;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal class SwitchCaseContainerFlow : SequenceContainerFlow
    {
        private readonly Func<ICodeFlowContext, object[], object> _switchExpression;
        private List<Func<object, object[], bool>> _switchSelectors = new List<Func<object, object[], bool>>();

        internal SwitchCaseContainerFlow(Func<ICodeFlowContext, object[], object> switchExpression)
        {
            _switchExpression = switchExpression;
        }
         
        public void Case(Func<object, object[], bool> switchSelector)
        {
            var caseContainer = new SwitchCaseSelectorContainerFlow(switchSelector, _switchExpression);
            if (!IsAllContainerClosed()) // close previous case statements
            {
                CloseContainer();
            }
            _switchSelectors.Add(switchSelector);
            AddContainer(caseContainer);
        } 

        public void Default()
        {
            var defaultContainer = new SwitchCaseDefaultContainerFlow(_switchSelectors, _switchExpression);
            if (!IsAllContainerClosed()) // close previous case statements
            {
                CloseContainer();
            }
            AddContainer(defaultContainer);
        }
    }

    internal class SwitchCaseSelectorContainerFlow : SequenceContainerFlow
    {
        private readonly Func<object, object[], bool> _switchSelector;
        private readonly Func<ICodeFlowContext, object[], object> _switchExpression;

        public SwitchCaseSelectorContainerFlow(Func<object, object[], bool> switchSelector, Func<ICodeFlowContext, object[], object> switchExpression)
        {
            _switchSelector = switchSelector;
            _switchExpression = switchExpression;
        }

        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0; 
            var switchRes = _switchExpression(context, inputs);
            if (_switchSelector(switchRes, inputs))
            {
                return base.Execute(context, activityFilter, inputs);
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode);
        }
    }

    internal class SwitchCaseDefaultContainerFlow : SequenceContainerFlow
    {
        private readonly List<Func<object, object[], bool>> _switchSelectors;
        private readonly Func<ICodeFlowContext, object[], object> _switchExpression;

        public SwitchCaseDefaultContainerFlow(List<Func<object, object[], bool>> switchSelectors, Func<ICodeFlowContext, object[], object> switchExpression)
        {
            _switchSelectors = switchSelectors;
            _switchExpression = switchExpression;
        }

        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0; 
            var switchRes = _switchExpression(context, inputs);
            if (_switchSelectors.All(x => x(switchRes, inputs) == false))
            {
                return base.Execute(context, activityFilter, inputs);
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode);
        }
    }

}