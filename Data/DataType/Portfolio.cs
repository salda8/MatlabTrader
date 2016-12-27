using System;
using System.Collections.Generic;

namespace MATLAB_trader.Data.DataType
{
    public class Portfolio
    {
        public static List<Portfolio> LivePortfolioTradesList = new List<Portfolio>();

        public Portfolio(string symbol, string contracts, string account)
        {
            Symbol = symbol.Substring(0, 3).Trim();
            Contracts = Convert.ToInt32(contracts);
            Account = account;
        }

        public string Account { get; set; }
        public int Contracts { get; set; }
        public string Symbol { get; set; }
    }
}