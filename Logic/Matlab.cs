//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using MATLAB_trader.Data;
//using MATLAB_trader.Data.DataType;

//namespace MATLAB_trader.Logic
//{
//    internal class Matlab
//    {
//        #region Constructor

//        /// <summary>
//        ///     Initializes a new instance of the <see cref="Matlab" /> class.
//        /// </summary>
//        public Matlab()
//        {
//            var activationContext = Type.GetTypeFromProgID("matlab.application.single");
//           // _matlab = (MLApp.MLApp) Activator.CreateInstance(activationContext);
//            _matlab.Visible = 1;
//        }

//        #endregion Constructor

//        internal class MatlabValue
//        {
//            private static readonly MatlabType _matlabValueType = MatlabType.Parameter;
//            public string Symbol { get; set; }
//            public double Value { get; set; }
//            public string Account { get; set; }
//            public double Equity { get; set; }
//            public MatlabType ValueType { get; set; } = _matlabValueType;
//            public string Name { get; set; }
//        }

//        internal class MatlabParameters : MatlabValue
//        {
//            private static readonly MatlabType _matlabValueType = MatlabType.Parameter;
//            public new MatlabType ValueType { get; set; } = _matlabValueType;
//        }

//        internal enum MatlabType
//        {
//            NumberOfContracts,
//            Parameter
//        }

//        #region Field

//        private static readonly List<MatlabValue> MatlabValues = new List<MatlabValue>();
//        private static readonly List<MatlabParameters> MatlabParametersList = new List<MatlabParameters>();
//        private static DateTime _lastUpdate;
//        private static bool _trade;
//        public static string Matlabexe = "[Vysledek2,DataImport,DataImport2] = CreatePort(a1,a2,a3,a4,a5,Equity, p)";
//        public static int CloseMinute = 59;
//        public static int CloseHour = 22;
//        public static int EurContracts;
//        public static int GbpContracts;
//        public static int JpyContracts;
//        public static int AudContracts;
//        public static int CadContracts;
//        //private readonly MLApp.MLApp _matlab;
//        private static readonly List<MinuteBar> UsedMinuteBarList = new List<MinuteBar>();
//        private static TaskFactory tf = new TaskFactory();

//        #endregion Field

//        //public void TestDll()
//        //{
//        //    double[,] a = { { 179440,179100,178860,179175,179040,179100,179250,179175,179275},
//        //        { 13749,13726,13716,137300,13746,137500,13731,137050,13706},
//        //        { 99450, 99325, 99138, 99269, 99125, 99131, 99150, 99113, 99150},
//        //    {72370,72270, 72140,72260,72270,72320,72420,72450, 72680},
//        //        {91870,91670,91670,92060,91930,91950,92040,91900,91940,
//        //     } }; //Matrix 1
//        //    //[,] b = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }; //Matrix 2
//        //    MWNumericArray arr1 = a;

//        //    //input = a;
//        //    var x = mydll.CreateEP((MWArray)arr1);

//        //    Console.WriteLine(x);
//        //}

//        #region Methods

//        /// <summary>
//        ///     Uses the matlab.
//        /// </summary>
//        public void UseMatlab(DateTime dt)
//        {
//            MatlabValues.Clear();
//            var barstoget = 20;

//            //Parallel.Invoke((() => GetData(barstoget)));
//            //Portfolio.LivePortfolioTradesList.Clear();
//            foreach (var wrapper in IbClient.WrapperList)
//            {
//                //OrderManager.SelectTradesFromDatabase(wrapper.AccountNumber);
//                GetAndPutWorkSpaceData(dt, wrapper);
//                //Console.WriteLine(_eur.Count);
//                //PutWorkSpaceData();
//                ExecuteMatLabFunctions();
//                // Alternative(wrapper);

//                var d = _matlab.GetVariable("Vysledek2", "base");

//                for (var i = 0; i < d.GetLength(0); i++)
//                {
//                    for (var j = 0; j < d.GetLength(1); j++)
//                    {
//                        switch (j)
//                        {
//                            case 0:

//                                EurContracts = (int) d.GetValue(i, j) - EurContracts;
//                                MatlabValues.Add(new MatlabValue
//                                {
//                                    Symbol = "EUR",
//                                    Value = d.GetValue(i, j),
//                                    Account = wrapper.AccountNumber,
//                                    Equity = wrapper.Equity
//                                });
//                                if (EurContracts == 0)
//                                {
//                                    EurContracts = (int) d.GetValue(i, j);
//                                    break;
//                                }
//                                Trade.PlaceTrade(MyContracts.GetContract("EUR"), EurContracts, wrapper);
//                                EurContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 1:

//                                GbpContracts = (int) d.GetValue(i, j) - GbpContracts;
//                                MatlabValues.Add(new MatlabValue
//                                {
//                                    Symbol = "GBP",
//                                    Value = d.GetValue(i, j),
//                                    Account = wrapper.AccountNumber,
//                                    Equity = wrapper.Equity
//                                });
//                                if (GbpContracts == 0)
//                                {
//                                    GbpContracts = (int) d.GetValue(i, j);
//                                    break;
//                                }
//                                Trade.PlaceTrade(MyContracts.GetContract("GBP"), GbpContracts, wrapper);
//                                GbpContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 2:
//                                JpyContracts = (int) d.GetValue(i, j) - JpyContracts;
//                                MatlabValues.Add(new MatlabValue
//                                {
//                                    Symbol = "JPY",
//                                    Value = d.GetValue(i, j),
//                                    Account = wrapper.AccountNumber,
//                                    Equity = wrapper.Equity
//                                });
//                                if (JpyContracts == 0)
//                                {
//                                    JpyContracts = (int) d.GetValue(i, j);
//                                    break;
//                                }
//                                Trade.PlaceTrade(MyContracts.GetContract("JPY"), JpyContracts, wrapper);
//                                JpyContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 3:
//                                AudContracts = (int) d.GetValue(i, j) - AudContracts;
//                                MatlabValues.Add(new MatlabValue
//                                {
//                                    Symbol = "AUD",
//                                    Value = d.GetValue(i, j),
//                                    Account = wrapper.AccountNumber,
//                                    Equity = wrapper.Equity
//                                });
//                                if (AudContracts == 0)
//                                {
//                                    AudContracts = (int) d.GetValue(i, j);
//                                    break;
//                                }
//                                Trade.PlaceTrade(MyContracts.GetContract("AUD"), AudContracts, wrapper);
//                                AudContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 4:
//                                CadContracts = (int) d.GetValue(i, j) - CadContracts;
//                                MatlabValues.Add(new MatlabValue
//                                {
//                                    Symbol = "CAD",
//                                    Value = d.GetValue(i, j),
//                                    Account = wrapper.AccountNumber,
//                                    Equity = wrapper.Equity
//                                });
//                                if (CadContracts == 0)
//                                {
//                                    CadContracts = (int) d.GetValue(i, j);
//                                    break;
//                                }
//                                Trade.PlaceTrade(MyContracts.GetContract("CAD"), CadContracts, wrapper);
//                                CadContracts = (int) d.GetValue(i, j);
//                                break;
//                        }
//                    }
//                }
//                foreach (var mv in MatlabValues)
//                {
//                    AddMatlabValueInDb(mv);
//                    Console.WriteLine(mv.Symbol + ": " + mv.Value + ": " + mv.Account);
//                }
//                foreach (var bar in UsedMinuteBarList)
//                {
//                    InsertUsedBarInDatabase(bar);
//                }
//                MatlabValues.Clear();
//            }
//        }

//        /// <summary>
//        ///     Alternatives the specified wrapper.
//        /// </summary>
//        /// <param name="wrapper">The wrapper.</param>
//        private void Alternative(IbClient wrapper)
//        {
//            var s = _matlab.GetVariable("Symbol", "base");
//            var d = _matlab.GetVariable("Vysledek", "base");
//            var pn = _matlab.GetVariable("PameterName", "base");
//            var p = _matlab.GetVariable("Parameter", "base");

//            for (var i = 0; i < d.GetLength(0); i++)
//            {
//                string symbol;
//                for (var j = 0; j < d.GetLength(1); j++)
//                {
//                    symbol = (string) s.GetValue(i, j);
//                    int contracts = Convert.ToInt16(Portfolio.LivePortfolioTradesList.FindLast(
//                        x => x.Account == wrapper.AccountNumber && x.Symbol == symbol).ToString());
//                    contracts = (int) d.GetValue(i, j) - contracts;
//                    if (contracts != 0)
//                    {
//                        Trade.PlaceTrade(MyContracts.GetContract(symbol), contracts, wrapper);
//                    }
//                    MatlabValues.Add(new MatlabValue
//                    {
//                        Symbol = symbol,
//                        Value = d.GetValue(i, j),
//                        Account = wrapper.AccountNumber,
//                        Equity = wrapper.Equity
//                    });

//                    //Portfolio.LivePortfolioTradesList.Add(new Portfolio(symbol, Convert.ToString((int)d.GetValue(i, j)), wrapper.AccountNumber));
//                }
//            }

//            for (var i = 0; i < pn.GetLength(0); i++)
//            {
//                for (var j = 0; j < pn.GetLength(1); j++)
//                {
//                    var parameter = new MatlabParameters
//                    {
//                        Name = (string) pn.GetValue(i, j),
//                        Value = (double) p.GetValue(i, j),
//                        Account = wrapper.AccountNumber
//                    };
//                    //AddMatlabParameterInDatabase(parameter);
//                    UpdateMatlabParameterInDatabase(parameter);
//                    MatlabParametersList.Add(parameter);
//                }
//            }
//            foreach (var mv in MatlabValues)
//            {
//                AddMatlabValueInDb(mv);
//                Console.WriteLine(mv.Symbol + ": " + mv.Value + ": " + mv.Account + " : " + mv.ValueType);
//            }
//            foreach (var bar in UsedMinuteBarList)
//            {
//                InsertUsedBarInDatabase(bar);
//            }

//            MatlabValues.Clear();
//            Portfolio.LivePortfolioTradesList.Clear();
//        }

//        private void UpdateMatlabParameterInDatabase(MatlabParameters mv)
//        {
//            using (var con = Db.OpenConnection())
//            {
//                using (var cmd = con.CreateCommand())
//                {
//                    cmd.CommandText =
//                        "UPDATE matlabvalues SET value=?value, date=?date WHERE account=?acc AND name=?name";
//                    cmd.Parameters.AddWithValue("?value", mv.Value);
//                    cmd.Parameters.AddWithValue("?date", DateTime.Now.Date);
//                    cmd.Parameters.AddWithValue("?acc", mv.Account);
//                    cmd.Parameters.AddWithValue("?name", mv.Name);
//                    var result = cmd.ExecuteNonQuery();
//                    if (result == 0)
//                    {
//                        AddMatlabParameterInDatabase(mv);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        ///     Adds the matlab parameter in database.
//        /// </summary>
//        /// <param name="mv">The matlab parameter.</param>
//        private void AddMatlabParameterInDatabase(MatlabParameters mv)
//        {
//            using (var con = Db.OpenConnection())
//            {
//                using (var cmd = con.CreateCommand())
//                {
//                    cmd.CommandText =
//                        "INSERT INTO matlabvalues (value, date, account, name) VALUES (?value,?date, ?acc,?name)";
//                    cmd.Parameters.AddWithValue("?value", mv.Value);
//                    cmd.Parameters.AddWithValue("?date", DateTime.Now.Date);
//                    cmd.Parameters.AddWithValue("?acc", mv.Account);
//                    cmd.Parameters.AddWithValue("?name", mv.Name);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        /// <summary>
//        ///     Adds the matlab value in database.
//        /// </summary>
//        /// <param name="mv">The value from matlab.</param>
//        private static void AddMatlabValueInDb(MatlabValue mv)
//        {
//            using (var con = Db.OpenConnection())
//            {
//                using (var cmd = con.CreateCommand())
//                {
//                    cmd.CommandText =
//                        "INSERT INTO matlabvalues (value, symbol, date, account, equity) VALUES (?value,?symbol, ?date, ?acc,?equity)";
//                    cmd.Parameters.AddWithValue("?value", mv.Value);
//                    cmd.Parameters.AddWithValue("?symbol", mv.Symbol);
//                    cmd.Parameters.AddWithValue("?date", DateTime.Now.Date);
//                    cmd.Parameters.AddWithValue("?acc", mv.Account);
//                    cmd.Parameters.AddWithValue("?equity", mv.Equity);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        /// <summary>
//        ///     Executes the mat lab functions.
//        /// </summary>
//        private void ExecuteMatLabFunctions()
//        {
//            _matlab.Execute(Matlabexe);
//        }

//        /// <summary>
//        ///     Gets the and put work space data.
//        /// </summary>
//        /// <param name="dt">The dt.</param>
//        /// <param name="wrapper">The wrapper.</param>
//        private void GetAndPutWorkSpaceData(DateTime dt, IbClient wrapper)
//        {
//            var x = 1;
//            foreach (var barSettings in Data.Data.BarSettings)
//            {
//                var timeframe = barSettings.Timeframe;
//                var period = barSettings.Period;
//                var symbol = barSettings.Symbol;
//                var value = barSettings.Value;
//                var list =
//                    Data.Data.RequestDataFromDb1(
//                        MyContracts.ContractsExtList.Find(b => b.TickerId.Substring(0, 2) == symbol)
//                            .TickerId.Substring(0, 2), timeframe, value, period);
//                for (var i = 0; i < 200; i++)
//                {
//                    if (Data.Data.BarCheck(dt, timeframe, list))
//                    {
//                        break;
//                    }
//                    if (i == 199)
//                    {
//                        var msg = "Barcheck failed on " + list[0].Symbol + "/" + list[0].TimeFrame + "||" + DateTime.Now;
//                        Console.WriteLine(msg);
//                        OrderManager.HandleErrorMsg(8888, msg, wrapper.AccountNumber);
//                    }
//                    else
//                    {
//                        list.Clear();
//                        list =
//                            Data.Data.RequestDataFromDb1(
//                                MyContracts.ContractsExtList.Find(b => b.TickerId.Substring(0, 2) == symbol)
//                                    .TickerId.Substring(0, 2), timeframe, value, period);
//                        Thread.Sleep(8);
//                    }
//                }

//                for (var j = 0; j < period; j++)
//                {
//                    var execute = "a" + x + "(" + (j + 1) + ")=" + list[j].Price;

//                    UsedMinuteBarList.Add(new MinuteBar(list[j].Price, list[j].Symbol, list[j].BarDateTime,
//                        list[j].TimeFrame));
//                    _matlab.Execute(execute);
//                }
//                x++;
//            }

//            _matlab.PutWorkspaceData("Equity", "base",
//                new[]
//                {wrapper.Equity*1d});
//            if (MatlabParametersList.Count > 0)
//            {
//                foreach (var matlabParameterse in MatlabParametersList)
//                {
//                    _matlab.PutWorkspaceData(matlabParameterse.Name, "base", matlabParameterse.Value*1d);
//                }
//            }
//            else
//            {
//                for (var j = 1; j < 11; j++)
//                {
//                    var execute = "p" + j + "=NaN";


//                    _matlab.Execute(execute);
//                }
//            }
//        }

//        /// <summary>
//        ///     Inserts the used bar in database.
//        /// </summary>
//        /// <param name="bar">The bar.</param>
//        private void InsertUsedBarInDatabase(MinuteBar bar)
//        {
//            using (var con = Db.OpenConnection())
//            {
//                using (var cmd = con.CreateCommand())
//                {
//                    cmd.CommandText =
//                        "INSERT INTO usedminutebars (close, symbol, timeframe, bartime) VALUES (?value,?symbol, ?timeframe, ?bartime)";
//                    cmd.Parameters.AddWithValue("?value", bar.Price);
//                    cmd.Parameters.AddWithValue("?symbol", bar.Symbol);
//                    cmd.Parameters.AddWithValue("?timeframe", bar.TimeFrame);
//                    cmd.Parameters.AddWithValue("?bartime", bar.BarDateTime);
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        /// <summary>
//        ///     Starts the trading.
//        /// </summary>
//        public static void StartTrading()
//        {
//            foreach (var set in AccountSettings.AccountSettingsList)
//            {
//                var wrapper = new IbClient();
//                wrapper.ClientSocket.eConnect("127.0.0.1", set.Port, 1, false);
//                wrapper.AccountNumber = set.AccountNumber;
//                while (wrapper.NextOrderId <= 0)
//                {
//                }
//                wrapper.ClientSocket.reqAccountUpdates(true, wrapper.AccountNumber);
//                IbClient.WrapperList.Add(wrapper);

//                OrderManager.SelectOpenOrdersFromDatabase(wrapper.AccountNumber);
//                OrderManager.SelectLiveTradeFromDatabase(wrapper.AccountNumber);
//                SelectMatlabParametersFromDatabase(wrapper.AccountNumber);
//            }

//            foreach (var symbol in Data.Data.SymbolsList)
//            {
//                MyContracts.GetContractsFromDbOnStartup(symbol);
//            }

//            Thread.Sleep(1000);

//            _trade = Db.GetTradeVariableOnStartUp();

//            var ml = new Matlab();

//            ml.UseMatlab(DateTime.Now.AddHours(-7));

//            while (true)
//            {
//                while (TradingCalendar.TradingDay())
//                {
//                    while (HighResolutionDateTime.UtcNow.Second != 0)
//                    {
//                        Thread.Sleep(1);
//                        //Console.WriteLine((DateTime.UtcNow.Millisecond));
//                    }
//                    if (DateTime.UtcNow.Minute == 0)
//                    {
//                        if (DateTime.Now.Hour != 23)
//                        {
//                            if (DateTime.Now.Hour == 0 &&
//                                DateTime.UtcNow.AddHours(-1).ToString("hh") != _lastUpdate.AddMinutes(2).ToString("hh"))
//                            {
//                                if (_trade)
//                                {
//                                    Thread.Sleep(1200);
//                                    Parallel.Invoke((() => ml.UseMatlab(DateTime.Now.AddHours(-7))));

//                                    _trade = false;
//                                    _lastUpdate = DateTime.UtcNow;
//                                }
//                                else
//                                {
//                                    _trade = true;
//                                    _lastUpdate = DateTime.UtcNow;
//                                }
//                                Db.InsertTadeInDb(_trade);
//                                Thread.Sleep(60000);
//                            }
//                        }
//                    }
//                    else if (DateTime.Now.Minute == 59 && DateTime.Now.Hour == 22)
//                    {
//                        if (_trade)
//                        {
//                            Thread.Sleep(1200);
//                            Parallel.Invoke((() => ml.UseMatlab(DateTime.Now.AddHours(-7))));

//                            _trade = false;
//                            _lastUpdate = DateTime.UtcNow;
//                        }
//                        else
//                        {
//                            _trade = true;
//                            _lastUpdate = DateTime.UtcNow;
//                        }
//                        Db.InsertTadeInDb(_trade);
//                        Thread.Sleep(60000);
//                    }
//                }
//                Thread.Sleep(10000);
//            }
//        }

//        /// <summary>
//        ///     Selects the matlab parameters from database.
//        /// </summary>
//        /// <param name="accountNumber">The account number.</param>
//        private static void SelectMatlabParametersFromDatabase(string accountNumber)
//        {
//            using (var con = Db.OpenConnection())
//            {
//                using (var cmd = con.CreateCommand())
//                {
//                    cmd.CommandText =
//                        "SELECT value, name FROM matlabvalue WHERE account=?acc AND valuetype=1";

//                    var result = cmd.ExecuteNonQuery();
//                    if (result > 0)
//                    {
//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                if (!Convert.IsDBNull(reader[0]) && !Convert.IsDBNull(reader[1]))
//                                {
//                                    MatlabParametersList.Add(new MatlabParameters
//                                    {
//                                        Account = accountNumber,
//                                        Name = reader.GetString(1),
//                                        Value = reader.GetDouble(0)
//                                    });
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        #endregion Methods
//    }
//}