using System.Collections.Generic;

namespace aceryan.codeflow.samples.ShowCase
{
    public interface IMarkToMarketService
    {
        List<TradeMarketToMarket> GetMarkToMarket(List<string> TradeIds);
    }

    public class MarkToMarketService : IMarkToMarketService
    {
        public List<TradeMarketToMarket> GetMarkToMarket(List<string> TradeIds)
        {
            return new List<TradeMarketToMarket>();
        }
    }
}
