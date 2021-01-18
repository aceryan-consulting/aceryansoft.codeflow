using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.model.Activities
{
    public interface ICodeFlowActivity
    {
        IExecutionContext Execute(ICodeFlowContext context, params object[] inputs);
    }
}
