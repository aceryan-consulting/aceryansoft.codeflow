using System;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Middlewares;

namespace aceryansoft.codeflow.test.TestModel
{
    public class SampleMiddleware : ICodeFlowMiddleWare
    {
        private readonly int _id;

        public SampleMiddleware(int id)
        {
            _id = id;
        }
        public void Execute(ICodeFlowContext context, Action next)
        { 
            context.As<SampleContext>().CallStack += $"middleware-mock-{_id}->"; 
            next();
        }
    }
}