using System;
using System.Collections.Generic;

namespace MATLAB_trader.Data.DataType
{
    public class HistoricalTrade
    {
        public static List<HistoricalTrade> HistoricalTradeList = new List<HistoricalTrade>();

        public HistoricalTrade(string account, string execId, DateTime time, string side, int qty, string description,
            double price, double commission, double pnl)
        {
            Account = account;
            ExecutionId = execId;
            ExecTime = time;
            Side = side;
            Quantity = qty;
            Description = description;
            Price = price;
            Commission = commission;
            RealizedPnL = pnl;
        }

        public string Account { get; set; }
        public string ExecutionId { get; set; }
        public string Description { get; set; }
        public string Side { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Commission { get; set; }
        public double RealizedPnL { get; set; }
        public DateTime ExecTime { get; set; }
    }
}