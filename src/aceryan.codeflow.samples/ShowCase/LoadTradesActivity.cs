using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.Activities;
using System.Linq;

namespace aceryan.codeflow.samples.ShowCase
{
    public class LoadTradesActivity : ICodeFlowActivity
    {
        private readonly ITradeSearchService _searchService;
        private readonly IMarkToMarketService _markToMarketService;

        public LoadTradesActivity(ITradeSearchService searchService, IMarkToMarketService markToMarketService)
        {
            _searchService = searchService;
            _markToMarketService = markToMarketService;
        }

        public IExecutionContext Execute(ICodeFlowContext context, params object[] inputs)
        {
            var searchCriteria = context.GetValue<SearchCriteria>("Criteria");
            var trades = _searchService.SearchTrade(searchCriteria);
            var mtms = _markToMarketService.GetMarkToMarket(trades.Select(x => x.TradeId).ToList());

            var joinedTrades = (from de in trades
                                 join mtm in mtms
                                     on de.TradeId equals mtm.TradeId into gmap
                                 from gmapMtm in gmap.DefaultIfEmpty()
                                 select new { Trade = de, Mtm = gmapMtm }).ToList();

            var tradeResult = joinedTrades.Select(x => {
            x.Trade.MarkToMarket = x.Mtm!=null ? x.Mtm.MarkToMarket : 0;
                return x.Trade;
            }).ToList();

            context.SetCollection<Trade>("Trades", tradeResult);
            return new ExecutionContext() { ActivityName= "LoadTrades", Status = Status.Succeeded };
        }
    }
}
