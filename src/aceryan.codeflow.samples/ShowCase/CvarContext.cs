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

        public CvarContext()
        {
            ContextProperties["Criteria"] = new ContextProperty(() => Criteria, (obj) => { Criteria = (SearchCriteria)obj; });
            ContextProperties["SimulationAlgorithm"] = new ContextProperty(() => SimulationAlgorithm, (obj) => { SimulationAlgorithm = obj?.ToString(); });
            ContextProperties["Trades"] = new ContextProperty(() => Trades, (obj) => { Trades = (List<Trade>)obj; });
            ContextProperties["CounterpartyParameters"] = new ContextProperty(() => CtyParameters, (obj) => { CtyParameters = (ConcurrentBag<CtyComputeParameter>)obj; });
            ContextProperties["CtyResults"] = new ContextProperty(() => CtyResults, (obj) => { CtyResults = (ConcurrentBag<CtyComputeResult>)obj; });
            ContextProperties["SharedInputs"] = new ContextProperty(() => SharedInputs, (obj) => { SharedInputs = (ConcurrentBag<SharedComputeInputs>)obj; });
        }
        public override T As<T>()
        {
            if (typeof(T) == typeof(CvarContext))
                return this as T;
            throw new NotImplementedException();
        }
    }
}
