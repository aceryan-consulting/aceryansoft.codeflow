using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aceryansoft.codeflow.core.Pipelines;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.Filter;
using aceryansoft.codeflow.model.Middlewares;

namespace aceryansoft.codeflow.core.Config
{
    /// <summary>
    /// code flow configuration
    /// </summary>
    internal class CodeFlowExecutionConfig : ICodeFlowExecutionConfig
    {
        private ICodeFlowContext _context;
        private readonly PipelineMiddleWare<ICodeFlowContext> _middlewarePipeline;
        private readonly PipelineFilter<ICodeFlowContext, IExecutionLog> _activityFilterPipeline;
        private Func<Type, string, object> _serviceResolver;

        /// <summary>
        /// define if the code flow is in rerun mode, if true flow start at the first matching restore point
        /// </summary>
        private bool _rerunMode;

        /// <summary>
        /// the id of the first restore point to launch
        /// </summary>
        private string _restorePointId = "";


        /// <summary>
        /// Create a new code flow configuration instance
        /// </summary>
        public CodeFlowExecutionConfig()
        {
            _context = new CodeFlowContext();
            _middlewarePipeline = new PipelineMiddleWare<ICodeFlowContext>();
            _activityFilterPipeline = new PipelineFilter<ICodeFlowContext, IExecutionLog>();           
        }

        public ICodeFlowExecutionConfig UseActivityFilter(Func<ICodeFlowContext, object[], Func<ICodeFlowContext, object[], IExecutionLog>, IExecutionLog> activityFilter)
        {
            _activityFilterPipeline.Use(activityFilter);
            return this;
        }
        
        public ICodeFlowExecutionConfig UseActivityFilter<T>(string instanceName = "") where T : ICodeFlowActivityFilter
        {
            if (_serviceResolver == null)
            {
                throw new ArgumentException($"Can't use middleware {nameof(T)} without serviceResolver");
            }
            var instance = (T)_serviceResolver(typeof(T), instanceName);

            _activityFilterPipeline.Use(instance.OnActivityExecuting, instance.OnActivityExecuted);
            return this;
        }

        public ICodeFlowExecutionConfig UseActivityFilter(ICodeFlowActivityFilter instance) 
        { 
            _activityFilterPipeline.Use(instance.OnActivityExecuting, instance.OnActivityExecuted);
            return this;
        }


        /// <summary>
        /// Add new middleware to code flow execution
        /// </summary>
        /// <param name="middleware">Action middleware definition</param>
        /// <returns></returns>
        public ICodeFlowExecutionConfig UseMiddleware(Action<ICodeFlowContext, Action> middleware)
        {
            _middlewarePipeline.Use(middleware);
            return this;
        }

        /// <summary>
        /// Add new middleware to code flow execution
        /// </summary>
        /// <typeparam name="T">type of the middleware</typeparam>
        /// <param name="instanceName">instance name if multiple instance registered</param>
        /// <returns>code flow configuration</returns>
        public ICodeFlowExecutionConfig UseMiddleware<T>(string instanceName = "") where T : ICodeFlowMiddleWare
        {
            if (_serviceResolver == null)
            {
                throw new ArgumentException($"Can't use middleware {nameof(T)} without serviceResolver");
            }
            var instance = (T)_serviceResolver(typeof(T), instanceName);
            return UseMiddleware((ctx, next) => { instance.Execute(ctx, next); });             
        }

        /// <summary>
        /// register ICfMiddleWare instance to the middleware pipeline
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public ICodeFlowExecutionConfig UseMiddleware(ICodeFlowMiddleWare middleware)
        {
            return UseMiddleware(middleware.Execute);
        }

        /// <summary>
        /// set up code flow context
        /// </summary>
        /// <param name="contextProvider"></param>
        /// <returns>code flow context shared between all activities</returns>
        public ICodeFlowExecutionConfig WithContext(Func<ICodeFlowContext> contextProvider)
        {
            _context = contextProvider();
            return this;
        }

        /// <summary>
        /// set up dependency resolver
        /// </summary>
        /// <param name="serviceResolver"></param>
        /// <returns>code flow configuration</returns>
        public ICodeFlowExecutionConfig WithServiceResolver(Func<Type, string, object> serviceResolver)
        {
            _serviceResolver = serviceResolver;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRerun"></param>
        /// <returns></returns>
        public ICodeFlowExecutionConfig RerunMode(bool isRerun)
        {
            _rerunMode = isRerun;
            return this; 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="restorePointId"></param>
        /// <returns></returns>
        public ICodeFlowExecutionConfig RestorePointId(string restorePointId)
        {
            _restorePointId = restorePointId;
            return this;
        }

        /// <summary>
        /// get code flow context
        /// </summary>
        /// <returns></returns>
        public ICodeFlowContext GetContext()
        {
            return _context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetRestorePointId()
        {
            return _restorePointId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRerunMode()
        {
            return _rerunMode;
        }

        /// <summary>
        /// get code flow context
        /// </summary>
        /// <returns></returns>
        public void SetContext(ICodeFlowContext context)
        {
            _context = context;
        }

        /// <summary>
        /// get service resolver
        /// </summary>
        /// <returns>service resolver</returns>
        public Func<Type, string, object> GetServiceResolver()
        {
            return _serviceResolver;
        }

        /// <summary>
        /// get code flow request pipeline middleware
        /// </summary>
        /// <param name="innerDelegate">code flow execute delegate</param>
        /// <returns>code flow request pipeline middleware</returns>
        public Action<ICodeFlowContext> GetMiddleWareRequestPipeline(Action<ICodeFlowContext> innerDelegate)
        {
            return _middlewarePipeline.GetRequestPipeline(innerDelegate); 
        }

        public ActivityDelegate GetActivityFilterPipeline(ActivityDelegate activity)
        {
            var activityWithFilterPipeline = _activityFilterPipeline.GetRequestPipeline((ctx, inputs) =>
            {
                var startDate = DateTime.Now;
                var executionContext = activity(ctx, inputs);
                var endDate = DateTime.Now;
                var result = new ExecutionLog()
                {
                    ActivityName = executionContext.ActivityName,
                    Status = executionContext.Status,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = startDate - endDate,
                    Error = executionContext.Error,
                    ErrorCode = executionContext.ErrorCode,
                    StartedBy = Environment.UserName
                };
                return result;
            });

            return (ctx, inputs) => activityWithFilterPipeline(ctx, inputs);
        }
    }
}
