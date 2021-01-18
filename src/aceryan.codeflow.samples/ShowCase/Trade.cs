using System;

namespace aceryan.codeflow.samples.ShowCase
{
    public class Trade
    {
        public string TradeId { get; set; }
        public string ProductType { get; set; }
        public string Way { get; set; }
        public DateTime VersionDate { get; set; }   
        public string Underlying { get; set; }     
        public string LegalEntity { get; set; }
        public string BookingEntity { get; set; }
        public decimal MarkToMarket { get; set; } 
        public bool IsOption { get; set; } 
    }
}
