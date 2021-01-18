using System.Linq;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Activities;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using Ninject;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class FuncTestFlowApiTest
    {
        [TestMethod]
        public void should_execute_func_operator_with_delegate_instance_and_dependency_injection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICodeFlowActivityFunction>().To<AddMeasureFuncActivity>(); 
            var instance1 = new AddMeasureFuncActivity();
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext() { LastIndex = 2 })
                        .WithServiceResolver((typ, str) => string.IsNullOrEmpty(str) ? kernel.Get(typ) : kernel.Get(typ, str));
                })
                .Function(IncrementActivityFuncDelegate)
                .WithResult<int>((ctx, inputs, res) =>
                {
                    ((SampleContext)ctx).LastIndex = res;
                })
                .Close()
                .Function(instance1)
                .WithArg<Measure>("Input0", (ctx, inputs) => new Measure() { Value = ((SampleContext)ctx).LastIndex }) // 3
                .WithArg<Measure>("Input1", (ctx, inputs) => new Measure() { Value = 5 })
                .WithResult<int>((ctx, inputs, res) =>
                {
                    ((SampleContext)ctx).LastIndex = res;
                })
                .Close()
                .Function<ICodeFlowActivityFunction>()
                .WithResult<int>((ctx, inputs, res) =>
                {
                    ((SampleContext)ctx).LastIndex += res;// 0
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            Check.That(instance1.Input0.Value).IsEqualTo(3);
            Check.That(instance1.Input1.Value).IsEqualTo(5);
            Check.That(contextResult.LastIndex).Equals(8);
        }

        private object IncrementActivityFuncDelegate(ICodeFlowContext context, params object[] inputs)
        {
            var ctxData = (SampleContext)context;
            return ctxData.LastIndex + 1;
        }

        public class AddMeasureFuncActivity : ICodeFlowActivityFunction
        {
            public Measure Input0 { get; set; }
            public Measure Input1 { get; set; }
            public object Execute(ICodeFlowContext context, params object[] inputs)
            {
                if (Input0 != null && Input1 != null)
                {
                    return Input0.Value + Input1.Value;
                }

                if (inputs?.Length >= 2)
                {
                    return inputs.OfType<Measure>().Sum(x => x.Value);
                }

                return 0; // or throw exception
            }
        }

        public class Measure
        {
            public int Value { get; set; }
        }
    }
}