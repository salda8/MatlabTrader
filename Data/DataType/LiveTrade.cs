using System;
using System.Collections.ObjectModel;

namespace MATLAB_trader.Data.DataType
{
    public class LiveTrade
    {
        public static ObservableCollection<LiveTrade> LiveTradeListCollection = new ObservableCollection<LiveTrade>();

        public LiveTrade(int permId, string accountNumber, int orderId, string secType, string symbol, int qty,
            string side, double avgFillPrice, DateTime time)
        {
            PermId = permId;
            ContractSymbol = symbol;
            ContractSecType = secType;
            AccountNumber = accountNumber;
            Qty = qty;
            Side = side;
            OrderId = orderId;
            AvgFillPrice = avgFillPrice;
            CurrentPrice = 0;
            OpenPnL = 0;
            Time = time;
        }

        public LiveTrade()
        {
        }

        public string AccountNumber { get; set; }
        public int PermId { get; set; }
        public int OrderId { get; set; }
        public string ContractSecType { get; set; }
        public string ContractSymbol { get; set; }
        public double OpenPnL { get; set; }
        public int Qty { get; set; }
        public string Side { get; set; }
        public double AvgFillPrice { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime Time { get; set; }
    }
}