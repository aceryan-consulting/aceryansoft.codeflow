using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.model.Config;

namespace aceryansoft.codeflow.test.TestModel
{
    public class SampleContext : CodeFlowContext
    {
        public string CallStack { get; set; } = "";
        public int LastIndex { get; set; }
        public ConcurrentBag<int> ThreadCallStack { get; set; } = new ConcurrentBag<int>();
        public ConcurrentBag<string> ConcurrentCallStack { get; set; } = new ConcurrentBag<string>();
    }
}
