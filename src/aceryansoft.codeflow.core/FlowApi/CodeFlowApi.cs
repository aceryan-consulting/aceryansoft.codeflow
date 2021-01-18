using System;
using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class CodeFlowApi : BaseFlowApi<ICodeFlowApi>, ICodeFlowApi
    {
        private bool _isRestorePointFound;

        internal CodeFlowApi(SequenceContainerFlow sequenceContainerFlow, CodeFlowExecutionConfig config)
            : base( sequenceContainerFlow, config)
        {
            SetReferenceFlowApiWithResult(this);
        }

        internal ICodeFlowContext Execute()
        {
            if (_config.IsRerunMode() && !_isRestorePointFound)
            {
                throw new NotImplementedException($"can't find restore point {_config.GetRestorePointId()} to rerun code flow.");
            }
            //Build code flow pipeline 
            var executionHandler = _config.GetMiddleWareRequestPipeline((ctx => _containerFlow.Execute(ctx, _config.GetActivityFilterPipeline)));

            var context = _config.GetContext();
            var startDate = DateTime.Now;
            executionHandler.Invoke(context); 
            context.StartDate = startDate;
            context.EndDate = DateTime.Now;
            context.Duration = context.StartDate - context.EndDate;
            return context;
        }

        public ICodeFlowApi RestorePoint(string restorePointId, Action<string, string, object[], ICodeFlowContext> contextSaver
            , Func<string, string, ICodeFlowContext> contextProvider)
        {
            if (_config.IsRerunMode())
            {
                if (_config.GetRestorePointId() == restorePointId && !_isRestorePointFound) // we found the first matching restoration point
                {
                    _isRestorePointFound = true;
                    _containerFlow = new SequenceContainerFlow(); // we clear the container to skip all previous registered activities
                    var context = _config.GetContext();
                    var restoredContext = contextProvider(context.RequestId, restorePointId);
                    _config.SetContext(restoredContext);
                }
            }
            else // save the current context for future rerun
            {
                Call((ctx, inputs) =>
                {
                    contextSaver(ctx.RequestId, restorePointId, inputs, ctx);
                });
            }
            return this;
        }

    }
}