using System;
using System.Collections.Generic;
using System.Linq;

namespace aceryansoft.codeflow.core.Pipelines
{
    internal class PipelineMiddleWare<T>
    {
        private readonly Stack<Func<Action<T>, Action<T>>> _pipelineStack = new Stack<Func<Action<T>, Action<T>>>();
        private void Use(Func<Action<T>, Action<T>> middleware)
        {
            _pipelineStack.Push(middleware); 
        }

        public void Use(Action<T, Action> middleware)
        {
            Use(next =>
            {
                return input => middleware(input, () => next(input));
            });
        }

        public Action<T> GetRequestPipeline(Action<T> pipelineEndpointDelegate)
        {
            foreach (var middleware in _pipelineStack.ToList())
            {
                pipelineEndpointDelegate = middleware(pipelineEndpointDelegate);
            }

            return pipelineEndpointDelegate;
        }
    }
}

