using System;
using System.Collections.Generic;
using MATLAB_trader.Data.DataType;

namespace MATLAB_trader.Data
{
    internal class Data
    {
        public static List<Tick> TickList = new List<Tick>();
        public static double LastPrice;
        public static List<MinuteBar> BarList = new List<MinuteBar>();
        public static List<BarData2> BarListData2 = new List<BarData2>();
        public static List<string> SymbolsList = new List<string> {"E6", "B6", "J6", "A6", "D6"};

        public static List<BarSettings> BarSettings = new List<BarSettings>
        {
            new BarSettings("E6", 60, 20, "close"),
            new BarSettings("B6", 60, 20, "close"),
            new BarSettings("J6", 60, 20, "close"),
            new BarSettings("A6", 60, 20, "close"),
            new BarSettings("D6", 60, 20, "close")
        };

        //public static int close;
        public static List<MinuteBar> RequestDataFromDb1(string tickerid, int tf, string ohlc, int limit = 100)
        {
            var data = new List<MinuteBar>();
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                switch (ohlc)
                {
                    case "open":
                        cmd.CommandText =
                            "SELECT open, bartime, volume FROM (SELECT * FROM minutebars WHERE symbol=?tickerid AND timeframe=?tf  ORDER BY bartime DESC LIMIT ?limit) as j ORDER by bartime ASC";
                        break;
                    case "high":
                        cmd.CommandText =
                            "SELECT high,bartime, volume FROM (SELECT * FROM minutebars WHERE symbol=?tickerid AND timeframe=?tf  ORDER BY bartime DESC LIMIT ?limit) as j ORDER by bartime ASC";
                        break;
                    case "low":
                        cmd.CommandText =
                            "SELECT low, bartime, volume FROM (SELECT * FROM minutebars WHERE symbol=?tickerid AND timeframe=?tf  ORDER BY bartime DESC LIMIT ?limit) as j ORDER by bartime ASC";
                        break;
                    case "close":
                        cmd.CommandText =
                            "SELECT close, bartime, volume FROM (SELECT * FROM minutebars WHERE symbol=?tickerid AND timeframe=?tf  ORDER BY bartime DESC LIMIT ?limit) as j ORDER by bartime ASC";
                        break;
                }

                cmd.Parameters.AddWithValue("?tickerid", tickerid);
                cmd.Parameters.AddWithValue("?limit", limit);
                cmd.Parameters.AddWithValue("?tf", tf);

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        var price = reader.GetDouble(0)*1d;

                        var datetime = reader.GetDateTime(1);
                        var volume = reader.GetInt32(2);

                        data.Add(new MinuteBar(price, volume, datetime, tickerid.Substring(0, 2), tf)
                            );
                    }
                }
            }

            return data;
        }

        /// <summary>
        ///     Requests the mkt data and history data on startup.
        /// </summary>
        /// <summary>
        ///     Requests the data from database.
        /// </summary>
        /// <param name="tickerid">The tickerid.</param>
        /// <param name="tf">The tf.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public static List<MinuteBar> RequestDataFromDb(string tickerid, int tf, int limit = 100)
        {
            var data = new List<MinuteBar>();
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT open,high,low,close, bartime, volume FROM (SELECT * FROM minutebars WHERE symbol=?tickerid AND timeframe=?tf  ORDER BY bartime DESC LIMIT ?limit) as j ORDER by bartime ASC";
                cmd.Parameters.AddWithValue("?tickerid", tickerid);
                cmd.Parameters.AddWithValue("?limit", limit);
                cmd.Parameters.AddWithValue("?tf", tf);

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        var price = 0d;
                        var open = reader.GetDouble(0)*1d;
                        var high = reader.GetDouble(1)*1d;
                        var low = reader.GetDouble(2)*1d;
                        var close = reader.GetDouble(3)*1d;
                        var datetime = reader.GetDateTime(4);
                        var volume = reader.GetInt32(5);


                        data.Add(new MinuteBar(open, high, low, close, volume, datetime, tickerid.Substring(0, 2), tf)
                            );
                    }
                }
            }

            return data;
        }

        public static bool BarCheck(DateTime dt, int tf, List<MinuteBar> barlist)
        {
            var check = false;
            switch (tf)
            {
                case 60:
                    check = CheckLastHour(dt, tf, barlist);
                    break;
                //case 1:
                //    break;
                //case 5:
                //    break;
                //case 10:
                //    break;
                //case 15:
                //    break;
                //case 30:
                //    break;
                case 120:
                    break;
                case 240:
                    break;
                case 720:
                    break;
                case 1440:
                    break;
                default:
                    check = CheckLastMinutes(dt, tf, barlist);
                    break;
            }

            return check;
        }

        private static bool CheckLastHour(DateTime dt, int tf, List<MinuteBar> barlist)
        {
            var check = false;
            var lastBar = barlist[barlist.Count - 1].BarDateTime;
            if (lastBar.AddHours(tf/60).Hour == dt.Hour)
            {
                check = true;
            }
            return check;
        }

        private static bool CheckLastMinutes(DateTime dt, int tf, List<MinuteBar> barlist)
        {
            var check = false;
            var lastBar = barlist[barlist.Count - 1].BarDateTime;
            if (lastBar.AddMinutes(tf).Minute == dt.Minute)
            {
                check = true;
            }
            return check;
        }

        /// <summary>
        ///     Reqs the request data2 list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static List<BarData2> RequestData2List(List<BarData2> list, int period)
        {
            var y = list.Count - period;

            var bar = new List<BarData2>();

            var x = 0;
            while (x < period)
            {
                bar.Add(list[y + x]);
                x++;
            }

            return bar;
        }
    }

    public class AccountSettings
    {
        public static List<AccountSettings> AccountSettingsList = new List<AccountSettings>();
        public string AccountNumber { get; set; }
        public int Port { get; set; }
    }

    internal class BarSettings
    {
        public BarSettings(string symbol, int timeframe, int period, string value)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            Period = period;
            Value = value;
        }

        public string Symbol { get; set; }
        public int Timeframe { get; set; }
        public int Period { get; set; }
        public string Value { get; set; }
    }
}