using System;
using System.Collections.Generic;
using aceryansoft.codeflow.model.Config;
using System.Collections.Concurrent;

namespace aceryan.codeflow.samples.ShowCase
{
    public class CtyComputeParameter
    {

    }
    public class CtyComputeResult
    {

    }

    public class SharedComputeInputs
    {

    }

    public class CvarContext : CodeFlowContext
    { 
        public ConcurrentBag<CtyComputeParameter> CtyParameters { get; set; } = new ConcurrentBag<CtyComputeParameter>();
        public ConcurrentBag<CtyComputeResult> CtyResults { get; set; } = new ConcurrentBag<CtyComputeResult>();
        public ConcurrentBag<SharedComputeInputs> SharedInputs { get; set; } = new ConcurrentBag<SharedComputeInputs>();
        public List<Trade> Trades { get; set; } = new List<Trade>();
        public SearchCriteria Criteria { get; set; }
        public string SimulationAlgorithm { get; set; } = "MonteCarlo";
         
    }
}
