using System;
using System.Collections.Generic;
using System.Linq;

namespace aceryansoft.codeflow.core.Pipelines
{
    internal class PipelineFilter<TData,TResult>
    {
        private readonly Stack<Func<Func<TData,object[], TResult>, Func<TData, object[], TResult>>> _pipelineStack = new Stack<Func<Func<TData, object[], TResult>, Func<TData, object[], TResult>>>();
        private void Use(Func<Func<TData, object[], TResult>, Func<TData, object[], TResult>> middleware)
        {
            _pipelineStack.Push(middleware);
        }

        public void Use(Func<TData, object[], Func<TData, object[], TResult>, TResult> middleware)
        {
            Use((next) =>
            {
                return (data, inputs) => middleware(data, inputs, next);  
            });
        }
        public void Use(Action<TData, object[]> onActivityExecuting, Action<TData, TResult, object[]> onActivityExecuted)
        {
            Use((data, inputs, next) =>
           { 
                onActivityExecuting(data, inputs);
                var res = next(data, inputs);
                onActivityExecuted(data, res, inputs);
                return res; 
           });
        }

        public Func<TData, object[], TResult> GetRequestPipeline(Func<TData, object[], TResult> activityEndpointDelegate)
        {
            foreach (var middleware in _pipelineStack.ToList())
            {
                activityEndpointDelegate = middleware(activityEndpointDelegate);
            }

            return activityEndpointDelegate;
        } 
    }
}
 