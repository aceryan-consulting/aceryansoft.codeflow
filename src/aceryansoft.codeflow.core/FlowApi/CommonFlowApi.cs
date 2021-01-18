using System;
using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Containers;
using aceryansoft.codeflow.model.Delegates;
using aceryansoft.codeflow.model.FlowApi;

namespace aceryansoft.codeflow.core.FlowApi
{
#pragma warning disable CS1066

    //todo find a way to refactor and reduce duplication 
    internal class CommonFlowApi<T> : ICommonFlowApi<T>
    {
        protected T _referenceFlowApi;
        protected IContainerFlow _containerFlow;
        protected readonly CodeFlowExecutionConfig _config;
        protected Func<Type, string, object> _serviceResolver;
        internal CommonFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config)
        {
            _containerFlow = containerFlow;
            _config = config;
            _serviceResolver = _config?.GetServiceResolver();
        }

        public void SetReferenceFlowApi(T referenceFlowApi)
        {
            _referenceFlowApi = referenceFlowApi;
        }

        public T Do(ActivityDelegate activity)
        {
            _containerFlow.AddActivity(activity);
            return _referenceFlowApi;
        }

        public T Do(ICodeFlowActivity instance)
        {
            return Do(instance.Execute);
        }

        public T Do<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivity
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return Do(instance);
        }

        public T Call(ActivityCallDelegate activity)
        {
            _containerFlow.AddActivity((context, inputs) =>
            {
                activity.Invoke(context, inputs);
                return new ExecutionContext { Status = Status.Succeeded };
            });
            return _referenceFlowApi;
        }

        public T Call(ICodeFlowActivityCall instance)
        {
            return Call(instance.Execute);
        }

        public T Call<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivityCall
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return Call(instance);
        }

        public IFunctionFlowApi<T> Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null)
        {
            return new FunctionFlowApi<T>(_referenceFlowApi, _containerFlow, activity, instance);
        }

        public IFunctionFlowApi<T> Function(ICodeFlowActivityFunction instance)
        {
            return Function(instance.Execute, instance);
        }

        public IFunctionFlowApi<T> Function<TInstance>(string instanceName = "") where TInstance : ICodeFlowActivityFunction
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return Function(instance);
        }
    }

    internal class CommonFlowApi<T1,T2> : CommonFlowApi<T1> ,ICommonFlowApi<T2>
    {
        protected T2 _referenceFlowApi2;
        internal CommonFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config) : base(containerFlow, config)
        {
        }
        public void Set2ReferenceFlowApi<TData>(TData referenceFlowApi) where TData : T1, T2
        {
            SetReferenceFlowApi(referenceFlowApi); 
            _referenceFlowApi2 = referenceFlowApi;
        }

        T2 ICommonFlowApi<T2>.Do(ActivityDelegate activity)
        {
            Do(activity);
            return _referenceFlowApi2;
        }

        T2 ICommonFlowApi<T2>.Do(ICodeFlowActivity instance)
        {
            Do(instance);
            return _referenceFlowApi2;
        }

        T2 ICommonFlowApi<T2>.Do<TInstance>(string instanceName = "")
        {
            Do<TInstance>(instanceName);
            return _referenceFlowApi2;
        }

        T2 ICommonFlowApi<T2>.Call(ActivityCallDelegate activity)
        {
            Call(activity);
            return _referenceFlowApi2;
        }

        T2 ICommonFlowApi<T2>.Call(ICodeFlowActivityCall instance)
        {
            Call(instance);
            return _referenceFlowApi2;
        }

        T2 ICommonFlowApi<T2>.Call<TInstance>(string instanceName = "") 
        {
            Call<TInstance>(instanceName);
            return _referenceFlowApi2;
        }

        IFunctionFlowApi<T2> ICommonFlowApi<T2>.Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null)
        {
            return new FunctionFlowApi<T2>(_referenceFlowApi2, _containerFlow, activity, instance);
        }

        IFunctionFlowApi<T2> ICommonFlowApi<T2>.Function(ICodeFlowActivityFunction instance)
        {
            return new FunctionFlowApi<T2>(_referenceFlowApi2, _containerFlow, instance.Execute, instance);
        }

        IFunctionFlowApi<T2> ICommonFlowApi<T2>.Function<TInstance>(string instanceName = "") 
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return new FunctionFlowApi<T2>(_referenceFlowApi2, _containerFlow, instance.Execute, instance);
        }
    }

    internal class CommonFlowApi<T1, T2,T3> : CommonFlowApi<T1,T2>, ICommonFlowApi<T3>
    {
        protected T3 _referenceFlowApi3;
        internal CommonFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config) : base(containerFlow, config)
        {
        }
        public void Set3ReferenceFlowApi<TData>(TData referenceFlowApi) where TData : T1, T2,T3
        {
            Set2ReferenceFlowApi(referenceFlowApi); 
            _referenceFlowApi3 = referenceFlowApi;
        }

        T3 ICommonFlowApi<T3>.Do(ActivityDelegate activity)
        {
            Do(activity);
            return _referenceFlowApi3;
        }

        T3 ICommonFlowApi<T3>.Do(ICodeFlowActivity instance)
        {
            Do(instance);
            return _referenceFlowApi3;
        }

        T3 ICommonFlowApi<T3>.Do<TInstance>(string instanceName = "")
        {
            Do<TInstance>(instanceName);
            return _referenceFlowApi3;
        }

        T3 ICommonFlowApi<T3>.Call(ActivityCallDelegate activity)
        {
            Call(activity);
            return _referenceFlowApi3;
        }

        T3 ICommonFlowApi<T3>.Call(ICodeFlowActivityCall instance)
        {
            Call(instance);
            return _referenceFlowApi3;
        }

        T3 ICommonFlowApi<T3>.Call<TInstance>(string instanceName = "")
        {
            Call<TInstance>(instanceName);
            return _referenceFlowApi3;
        }

        IFunctionFlowApi<T3> ICommonFlowApi<T3>.Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null)
        {
            return new FunctionFlowApi<T3>(_referenceFlowApi3, _containerFlow, activity, instance);
        }

        IFunctionFlowApi<T3> ICommonFlowApi<T3>.Function(ICodeFlowActivityFunction instance)
        {
            return new FunctionFlowApi<T3>(_referenceFlowApi3, _containerFlow, instance.Execute, instance);
        }

        IFunctionFlowApi<T3> ICommonFlowApi<T3>.Function<TInstance>(string instanceName = "")
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return new FunctionFlowApi<T3>(_referenceFlowApi3, _containerFlow, instance.Execute, instance);
        }
    }

    internal class CommonFlowApi<T1, T2, T3,T4> : CommonFlowApi<T1, T2,T3>, ICommonFlowApi<T4>
    {
        protected T4 _referenceFlowApi4;
        internal CommonFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config) : base(containerFlow, config)
        {
        }
        public void Set4ReferenceFlowApi<TData>(TData referenceFlowApi) where TData : T1, T2, T3,T4
        {
            Set3ReferenceFlowApi(referenceFlowApi); 
            _referenceFlowApi4 = referenceFlowApi;
        }

        T4 ICommonFlowApi<T4>.Do(ActivityDelegate activity)
        {
            Do(activity);
            return _referenceFlowApi4;
        }

        T4 ICommonFlowApi<T4>.Do(ICodeFlowActivity instance)
        {
            Do(instance);
            return _referenceFlowApi4;
        }

        T4 ICommonFlowApi<T4>.Do<TInstance>(string instanceName = "")
        {
            Do<TInstance>(instanceName);
            return _referenceFlowApi4;
        }

        T4 ICommonFlowApi<T4>.Call(ActivityCallDelegate activity)
        {
            Call(activity);
            return _referenceFlowApi4;
        }

        T4 ICommonFlowApi<T4>.Call(ICodeFlowActivityCall instance)
        {
            Call(instance);
            return _referenceFlowApi4;
        }

        T4 ICommonFlowApi<T4>.Call<TInstance>(string instanceName = "")
        {
            Call<TInstance>(instanceName);
            return _referenceFlowApi4;
        }

        IFunctionFlowApi<T4> ICommonFlowApi<T4>.Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null)
        {
            return new FunctionFlowApi<T4>(_referenceFlowApi4, _containerFlow, activity, instance);
        }

        IFunctionFlowApi<T4> ICommonFlowApi<T4>.Function(ICodeFlowActivityFunction instance)
        {
            return new FunctionFlowApi<T4>(_referenceFlowApi4, _containerFlow, instance.Execute, instance);
        }

        IFunctionFlowApi<T4> ICommonFlowApi<T4>.Function<TInstance>(string instanceName = "")
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return new FunctionFlowApi<T4>(_referenceFlowApi4, _containerFlow, instance.Execute, instance);
        }
    }


    internal class CommonFlowApi<T1, T2, T3, T4,T5> : CommonFlowApi<T1, T2, T3,T4>, ICommonFlowApi<T5>
    {
        protected T5 _referenceFlowApi5;
        internal CommonFlowApi(IContainerFlow containerFlow, CodeFlowExecutionConfig config) : base(containerFlow, config)
        {
        }
        public void Set5ReferenceFlowApi<TData>(TData referenceFlowApi) where TData : T1, T2, T3, T4,T5
        {
            Set4ReferenceFlowApi(referenceFlowApi);
            _referenceFlowApi5 = referenceFlowApi;
        }

        T5 ICommonFlowApi<T5>.Do(ActivityDelegate activity)
        {
            Do(activity);
            return _referenceFlowApi5;
        }

        T5 ICommonFlowApi<T5>.Do(ICodeFlowActivity instance)
        {
            Do(instance);
            return _referenceFlowApi5;
        }

        T5 ICommonFlowApi<T5>.Do<TInstance>(string instanceName = "")
        {
            Do<TInstance>(instanceName);
            return _referenceFlowApi5;
        }

        T5 ICommonFlowApi<T5>.Call(ActivityCallDelegate activity)
        {
            Call(activity);
            return _referenceFlowApi5;
        }

        T5 ICommonFlowApi<T5>.Call(ICodeFlowActivityCall instance)
        {
            Call(instance);
            return _referenceFlowApi5;
        }

        T5 ICommonFlowApi<T5>.Call<TInstance>(string instanceName = "")
        {
            Call<TInstance>(instanceName);
            return _referenceFlowApi5;
        }

        IFunctionFlowApi<T5> ICommonFlowApi<T5>.Function(ActivityFunctionDelegate activity, ICodeFlowActivityFunction instance = null)
        {
            return new FunctionFlowApi<T5>(_referenceFlowApi5, _containerFlow, activity, instance);
        }

        IFunctionFlowApi<T5> ICommonFlowApi<T5>.Function(ICodeFlowActivityFunction instance)
        {
            return new FunctionFlowApi<T5>(_referenceFlowApi5, _containerFlow, instance.Execute, instance);
        }

        IFunctionFlowApi<T5> ICommonFlowApi<T5>.Function<TInstance>(string instanceName = "")
        {
            var instance = (TInstance)_serviceResolver.Invoke(typeof(TInstance), instanceName);
            return new FunctionFlowApi<T5>(_referenceFlowApi5, _containerFlow, instance.Execute, instance);
        }
    }


#pragma warning restore CS1066
}