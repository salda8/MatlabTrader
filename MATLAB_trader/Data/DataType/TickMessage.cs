using System;

namespace MATLAB_trader.Data.DataType
{
    public class Tick
    {
        public Tick(double lPrice)
        {
            LastPrice = lPrice;
        }

        public Tick(double lPrice, DateTime time, string symbol)
        {
            LastPrice = lPrice;
            TickTime = time;
            Symbol = symbol;
        }

        public string Symbol { get; set; }
        public double LastPrice { get; set; }
        public DateTime TickTime { get; set; }
    }
}