using System;
using System.Collections.Generic;
using System.Linq;
using aceryansoft.codeflow.core.Containers;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
    internal class CodeFlowFunctionArg
    {
        public string PropertyName { get; set; }
        public Type DataType { get; set; }
        public Func<ICodeFlowContext, object[], object> ArgValueProvider { get; set; }
    }

    internal class CodeFlowFunctionResult
    {
        public Type DataType { get; set; }
        public Action<ICodeFlowContext, object[], object> ResultCallback { get; set; }
    }

    internal class FunctionFlowApi<T> : IFunctionFlowApi<T>, ICloseFunctionFlowApi<T>
    {
        private readonly T _parentBlockPath;
        private readonly IContainerFlow _parentcontainerFlow;
        private readonly ActivityFunctionDelegate _activityFunctionDelegate;
        private readonly ICodeFlowActivityFunction _codeFlowFuncInstance;
        private Queue<CodeFlowFunctionArg> _functionArgsQueue = new Queue<CodeFlowFunctionArg>();
        private CodeFlowFunctionResult _codeFlowFunctionResult;


        internal FunctionFlowApi(T parentBlockPath, IContainerFlow parentcontainerFlow
            , ActivityFunctionDelegate activityFunctionDelegate , ICodeFlowActivityFunction codeFlowFuncInstance = null)
        {
            _parentBlockPath = parentBlockPath;
            _parentcontainerFlow = parentcontainerFlow;
            _activityFunctionDelegate = activityFunctionDelegate;
            _codeFlowFuncInstance = codeFlowFuncInstance;
        }

        public IFunctionFlowApi<T> WithArg<TArg>(string propertyName, Func<ICodeFlowContext, object[], TArg> argValueProvider)
        {
            _functionArgsQueue.Enqueue(new CodeFlowFunctionArg() { PropertyName = propertyName.ToLower(), DataType = typeof(TArg), ArgValueProvider = (ctx, inputs) => argValueProvider(ctx, inputs) });
            return this;
        }

        public ICloseFunctionFlowApi<T> WithResult<TResult>(Action<ICodeFlowContext, object[], TResult> resultCallback)
        {
            _codeFlowFunctionResult = new CodeFlowFunctionResult()
            {
                DataType = typeof(T),
                ResultCallback = (ctx,inputs, result) => resultCallback(ctx,inputs , (TResult)result)
            };
            return this;
        }

        public T Close()
        {
            var propertiesSetup = GetPropertiesSetup();

            var resultActivity = GetFuncResultActivityDelegate(propertiesSetup);
            _parentcontainerFlow.AddActivity(resultActivity);
            return _parentBlockPath;
        }

        private ActivityDelegate GetFuncResultActivityDelegate(List<Action<ICodeFlowContext, object[]>> propertiesSetup)
        {
            ActivityDelegate resultActivity = (context, inputs) =>
            {
                foreach (var propertySetup in propertiesSetup)
                {
                    propertySetup(context, inputs);
                }

                var initialParameters = inputs != null && inputs.Any() ? inputs.ToList() : new List<object>();
                foreach (var funcArg in _functionArgsQueue)
                {
                    initialParameters.Add(funcArg.ArgValueProvider(context, inputs));
                }

                var funcResult = _activityFunctionDelegate(context, initialParameters.ToArray());
                _codeFlowFunctionResult.ResultCallback(context, inputs, funcResult);
                return new ExecutionContext { Status = Status.Succeeded };
            };
            return resultActivity;
        }

        private List<Action<ICodeFlowContext, object[]>> GetPropertiesSetup()
        {
            var propertiesSetup = new List<Action<ICodeFlowContext, object[]>>();
            if (_codeFlowFuncInstance != null)
            {
                var instanceProperties = _codeFlowFuncInstance.GetType().GetProperties().ToList();
                foreach (var funcArg in _functionArgsQueue)
                {
                    var matchingArg = instanceProperties.FirstOrDefault(x =>
                        x.Name.ToLower() == funcArg.PropertyName && x.PropertyType == funcArg.DataType);
                    if (matchingArg != null)
                    {
                        propertiesSetup.Add((ctx,inputs) => matchingArg.SetValue(_codeFlowFuncInstance, funcArg.ArgValueProvider(ctx, inputs)));
                    }
                }
            }

            return propertiesSetup;
        }

    }
}