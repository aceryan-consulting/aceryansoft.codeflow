using aceryansoft.codeflow.model.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace aceryansoft.codeflow.model.Filter
{
    public interface ICodeFlowActivityFilter
    {
        void OnActivityExecuting(ICodeFlowContext context, params object[] inputs);
        void OnActivityExecuted(ICodeFlowContext context, IExecutionLog executionLog, params object[] inputs);
    }
}
