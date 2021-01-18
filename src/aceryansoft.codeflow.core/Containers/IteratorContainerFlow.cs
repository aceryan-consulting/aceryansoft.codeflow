using System;
using System.Collections.Generic;
using aceryansoft.codeflow.core.Helpers;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;

namespace aceryansoft.codeflow.core.Containers
{
    internal abstract class IteratorContainerFlow : BaseContainerFlow
    {
        protected readonly Func<ICodeFlowContext, object[], List<object>> _itemsIterator;
        protected readonly int _packetSize;

        public IteratorContainerFlow(Func<ICodeFlowContext, object[], List<object>> itemsIterator, int packetSize = 0)
        {
            _itemsIterator = itemsIterator;
            _packetSize = packetSize;
        }
        public override IExecutionContext Execute(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            if (_packetSize == 0)
            {
                return ExecuteOnItems(context, activityFilter, _itemsIterator(context, inputs), inputs);
            }
            var childResults = new List<IExecutionContext>();
            int lastErrorCode = 0;
            foreach (var itemGroup in _itemsIterator(context, inputs).SplitByPacket<object>(_packetSize))
            {

                var innerRes = ExecuteOnItems(context, activityFilter, itemGroup, inputs);
                childResults.Add(innerRes);
                if (innerRes.Status == Status.Failed && !_config.ContinueOnError)
                {
                    lastErrorCode = innerRes.ErrorCode;
                    break;
                }
            }
            return ExecutionContext.GetCombinedSequenceResult(childResults, lastErrorCode); 
        }

        protected IExecutionContext ExecuteForEach(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter, params object[] inputs)
        {
            var innerResults = new List<IExecutionContext>();
            int innerErrorCode = 0;
            foreach (var element in PipelineQueue)
            {
                var actResult = element.Execute(context, activityFilter, inputs);
                innerResults.Add(actResult);
                if (actResult.Status == Status.Failed && !_config.ContinueOnError)
                {
                    innerErrorCode = actResult.ErrorCode;
                    break;
                }
            }
            return ExecutionContext.GetCombinedSequenceResult(innerResults, innerErrorCode);
        }

        protected abstract IExecutionContext ExecuteOnItems(ICodeFlowContext context, Func<ActivityDelegate, ActivityDelegate> activityFilter
            , List<object> items, params object[] inputs); 
    }
}