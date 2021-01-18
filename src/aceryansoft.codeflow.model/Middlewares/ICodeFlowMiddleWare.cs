using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.model.Middlewares
{ 

    public interface ICodeFlowMiddleWare
    {
        void Execute(ICodeFlowContext context, Action next);
    }
}
