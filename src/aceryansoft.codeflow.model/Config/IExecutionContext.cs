using System.Collections.Generic;
using System.Linq;

namespace aceryansoft.codeflow.model.Config
{
    public interface IExecutionContext
    {
        string ActivityName { get; set; }
        string Error { get; set; }
        int ErrorCode { get; set; }
        Status Status { get; set; }
    }

    public class ExecutionContext : IExecutionContext
    {
        public string ActivityName { get; set; } = "";
        public string Error { get; set; } = "";
        public int ErrorCode { get; set; }
        public Status Status { get; set; }

        public static IExecutionContext GetCombinedSequenceResult(List<IExecutionContext> childResults, int lastErrorCode)
        {
            var result = new ExecutionContext
            {
                Status = Status.Succeeded,
                ErrorCode = lastErrorCode,
                Error = string.Join(" | ", childResults.Select(x => x.Error))
            };
            if (childResults.Any(x => x.Status == Status.Failed))
            {
                result.Status = Status.Failed;
            }

            return result;
        }

        public static ExecutionContext GetDefault()
        {
            return new ExecutionContext()
            {
                Status = Status.Pending,
                Error = "",
                ErrorCode = 0
            };
        }

    }
}