using System;

namespace aceryansoft.codeflow.model.Config
{
    public interface IExecutionLog : IExecutionContext
    {
        string StartedBy { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        TimeSpan Duration { get; set; }
    }

    public class ExecutionLog : ExecutionContext, IExecutionLog
    {
        public string StartedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Duration { get; set; }
    }
}