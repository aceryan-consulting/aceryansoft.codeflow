using System.Collections.Generic;

namespace aceryan.codeflow.samples.ShowCase
{
    public interface ITradeSearchService
    {
        List<Trade> SearchTrade(SearchCriteria criteria);
    }

    public class TradeSearchService : ITradeSearchService
    {
        public List<Trade> SearchTrade(SearchCriteria criteria)
        {
            return new List<Trade>();
        }
    }
}
