using System;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.FlowApi;
using aceryansoft.codeflow.test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace aceryansoft.codeflow.test
{
    [TestClass]
    public class RestorePointFlowApiTest
    {

        [TestMethod]
        public void should_throw_exception_if_no_restore_point_found_in_rerun_mode()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext()).RerunMode(true).RestorePointId("some_id"); 
                })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .CallCodeFlow(GetInnerCodeFlow)
                .RestorePoint("other_id", (reqId, pointId, inputs, ctx) => { },(reqId,pointId)=> new SampleContext())
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                }) 
                .Close();

            Check.ThatCode(() => codeFlow.Execute()).Throws<NotImplementedException>();  
        }

        [TestMethod]
        public void should_restore_code_flow_on_first_expected_point_in_rerun_mode()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
                {
                    cfg.WithContext(() => new SampleContext()).RerunMode(true).RestorePointId("some_id"); 
                })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .CallCodeFlow(GetInnerCodeFlow)
                .RestorePoint("other_id_1", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .RestorePoint("some_id", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "some_id-1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .RestorePoint("other_id_2", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do3|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .RestorePoint("some_id", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "some_id-2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            Check.That(contextResult.CallStack).Equals("get-restore-point-some_id|some_id-1|do3|some_id-2|");
        }


        [TestMethod]
        public void should_save_restoration_context_when_not_in_rerun_mode()
        {
            var codeFlow = new CodeFlow();
            codeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() => new SampleContext()).RerunMode(false).RestorePointId("some_id"); 
            })
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .CallCodeFlow(GetInnerCodeFlow)
                .RestorePoint("other_id_1", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "do2|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .RestorePoint("some_id", SaveCodeFlowConfig, GetRestoredCodeFlowConfig)
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "some_id-1|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();

            var contextResult = (SampleContext)codeFlow.Execute();
            Check.That(contextResult.CallStack.Contains("save-restore-point-other_id_1|")).IsTrue();
            Check.That(contextResult.CallStack.Contains("save-restore-point-some_id|")).IsTrue();
            Check.That(contextResult.CallStack).Equals("do1|call-init|call-If|call-sub-do|save-restore-point-other_id_1|do2|save-restore-point-some_id|some_id-1|"); 
        }

        private void SaveCodeFlowConfig(string reqId, string pointId,object[] inputs, ICodeFlowContext ctx)
        {
            ((SampleContext) ctx).CallStack += $"save-restore-point-{pointId}|";
        }

        private CodeFlowContext GetRestoredCodeFlowConfig(string reqId,string pointId)
        {
            return new SampleContext() {CallStack = $"get-restore-point-{pointId}|"};
        }
 
        private void GetInnerCodeFlow(ICallFlowApi blockPath)
        {
            blockPath
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext) ctx;
                    ctxData.CallStack += "call-init|";
                    return new ExecutionContext() {Status = Status.Succeeded};
                })
                .If((ctx, inputs) => ((SampleContext) ctx).CallStack.Contains("call-init|"))
                    .Do((ctx, inputs) =>
                    {
                        var ctxData = (SampleContext) ctx;
                        ctxData.CallStack += "call-If|";
                        return new ExecutionContext() {Status = Status.Succeeded};
                    })
                .Close()
                .CallCodeFlow(GetSubInnerCodeFlow);
        }

        private void GetSubInnerCodeFlow(ICallFlowApi blockPath)
        {
            blockPath
                .Do((ctx, inputs) =>
                {
                    var ctxData = (SampleContext)ctx;
                    ctxData.CallStack += "call-sub-do|";
                    return new ExecutionContext() { Status = Status.Succeeded };
                })
                .Close();
        }


    }
}