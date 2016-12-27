using System;

namespace MATLAB_trader.Data.DataType
{
    public class MinuteBar
    {
        public MinuteBar(double o, double h, double l, double c, double vol, DateTime barDate)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            BarDateTime = barDate;
        }

        public MinuteBar(double o, double h, double l, double c, double vol, DateTime barDate, string symbol)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            BarDateTime = barDate;
            Symbol = symbol;
        }

        public MinuteBar(double o, double h, double l, double c, double vol, DateTime barDate, string symbol, int tf)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = vol;
            BarDateTime = barDate;
            Symbol = symbol;
            TimeFrame = tf;
        }

        public MinuteBar(double price, string tickerid)
        {
            Close = price;
        }

        public MinuteBar(double price, int volume, DateTime datetime, string symbol, int tf)
        {
            Price = price;
            Volume = volume;
            BarDateTime = datetime;
            Symbol = symbol;
            TimeFrame = tf;
        }

        public MinuteBar(double price, string tickerid, DateTime barDateTime, int timeFrame)
        {
            Price = price;
            BarDateTime = barDateTime;
            Symbol = tickerid;
            TimeFrame = timeFrame;
        }

        public int TimeFrame { get; set; }
        public string Symbol { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public DateTime BarDateTime { get; set; }
    }
}