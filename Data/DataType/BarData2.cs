using System;

namespace MATLAB_trader.Data.DataType
{
    public class BarData2
    {
        public BarData2(double o, double h, double l, double c, int vol)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            //BarDateTime = barDate;
        }

        public BarData2(double o, double h, double l, double c, int vol, DateTime barDate)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            BarDateTime = barDate;
        }

        public BarData2(double o, double h, double l, double c, int vol, DateTime barDate, string symbol)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            BarDateTime = barDate;
            Symbol = symbol;
        }

        public BarData2(double price, DateTime getDateTime)
        {
            Close = price;
        }

        public string Symbol { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public DateTime BarDateTime { get; set; }
    }
}