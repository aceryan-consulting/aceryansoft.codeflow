using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using aceryansoft.codeflow.model.Config;
 
namespace aceryansoft.codeflow.model.Delegates
{
    public delegate IExecutionContext ActivityDelegate(ICodeFlowContext context, params object[] inputs);
}
