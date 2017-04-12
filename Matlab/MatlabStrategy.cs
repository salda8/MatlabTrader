//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Common;
//using MATLAB_trader.Data;
//using MATLAB_trader.Data.DataType;
//using MATLAB_trader.Data.Matlab;
//using MATLAB_trader.Logic;

//namespace MATLAB_trader.Matlab
//{
//    internal class MatlabStrategy : IStrategy
//    {
//        private readonly MLApp.MLApp matlab;


//        /// <summary>
//        ///     Initializes a new instance of the <see cref="MatlabStrategy" /> class.
//        /// </summary>
//        public MatlabStrategy(MLApp.MLApp matlab)
//        {
//            this.matlab = matlab;
//            matlab.Visible = 1;
//        }

//        private readonly List<MatlabValue> matlabValues = new List<MatlabValue>();
//        private readonly List<MatlabParameter> matlabParametersList = new List<MatlabParameter>();
//        private static DateTime lastUpdate;
//        private static bool trade;
//        public static int CloseMinute = 59;
//        public static int CloseHour = 22;
//        public static int EurContracts;
//        public static int GbpContracts;
//        public static int JpyContracts;
//        public static int AudContracts;
//        public static int CadContracts;
//        //private readonly MLApp.MLApp matlab;
//        private static readonly List<OHLCBar> UsedMinuteBarList = new List<OHLCBar>();
//        private static TaskFactory tf = new TaskFactory();

//        public static string MatlabFunction { get; set; } =
//            "[Vysledek2,DataImport,DataImport2] = CreatePort(a1,a2,a3,a4,a5,Equity, p)";

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


//        /// <summary>
//        ///     Uses the matlab.
//        /// </summary>
//        public void Execute()
//        {
//            matlabValues.Clear();

//            //Parallel.Invoke((() => GetData(barstoget)));
//            //Portfolio.LivePortfolioTradesList.Clear();
//            foreach (var wrapper in IbClient.WrapperList)
//            {
//                //NetMqMessanger.SelectTradesFromDatabase(wrapper.AccountNumber);
//                var dt = DateTime.Now;
//                GetAndPutWorkSpaceData(dt, wrapper);
//                //Console.WriteLine(_eur.Count);
//                //PutWorkSpaceData();
//                ExecuteMatLabFunctions();
//                // Alternative(wrapper);

//                var d = matlab.GetVariable("Vysledek2", "base");

//                for (var i = 0; i < d.GetLength(0); i++)
//                {
//                    for (var j = 0; j < d.GetLength(1); j++)
//                    {
//                        switch (j)
//                        {
//                            case 0:

//                                EurContracts = (int) d.GetValue(i, j) - EurContracts;
//                                matlabValues.Add(new MatlabNumberOfContracts
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
//                                //Trade.PlaceTrade(MyContracts.GetContract("EUR"), EurContracts, wrapper);
//                                EurContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 1:

//                                GbpContracts = (int) d.GetValue(i, j) - GbpContracts;
//                                matlabValues.Add(new MatlabNumberOfContracts
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
//                                //Trade.PlaceTrade(MyContracts.GetContract("GBP"), GbpContracts, wrapper);
//                                GbpContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 2:
//                                JpyContracts = (int) d.GetValue(i, j) - JpyContracts;
//                                matlabValues.Add(new MatlabNumberOfContracts
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
//                                //Trade.PlaceTrade(MyContracts.GetContract("JPY"), JpyContracts, wrapper);
//                                JpyContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 3:
//                                AudContracts = (int) d.GetValue(i, j) - AudContracts;
//                                matlabValues.Add(new MatlabNumberOfContracts
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
//                                //Trade.PlaceTrade(MyContracts.GetContract("AUD"), AudContracts, wrapper);
//                                AudContracts = (int) d.GetValue(i, j);
//                                break;

//                            case 4:
//                                CadContracts = (int) d.GetValue(i, j) - CadContracts;
//                                matlabValues.Add(new MatlabNumberOfContracts
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
//                                //Trade.PlaceTrade(MyContracts.GetContract("CAD"), CadContracts, wrapper);
//                                CadContracts = (int) d.GetValue(i, j);
//                                break;
//                        }
//                    }
//                }

                
                
//                matlabValues.Clear();
//            }
//        }

//        /// <summary>
//        ///     Alternatives the specified wrapper.
//        /// </summary>
//        /// <param name="wrapper">The wrapper.</param>
//        private void Alternative(IbClient wrapper)
//        {
//            var theSymbol = matlab.GetVariable("Symbol", "base");
//            var result = matlab.GetVariable("Vysledek", "base");
//            var parameterName = matlab.GetVariable("PameterName", "base");
//            var parameterValue = matlab.GetVariable("Parameter", "base");

//            for (var i = 0; i < result.GetLength(0); i++)
//            {
//                string symbol;
//                for (var j = 0; j < result.GetLength(1); j++)
//                {
//                    symbol = (string) theSymbol.GetValue(i, j);
//                    int contracts = Convert.ToInt16(Portfolio.LivePortfolioTradesList.FindLast(
//                        x => x.Account == wrapper.AccountNumber && x.Symbol == symbol).ToString());
//                    contracts = (int) result.GetValue(i, j) - contracts;
//                    if (contracts != 0)
//                    {
//                        //Trade.PlaceTrade(MyContracts.GetContract(symbol), contracts, wrapper);
//                    }
//                    matlabValues.Add(new MatlabNumberOfContracts
//                    {
//                        Symbol = symbol,
//                        Value = result.GetValue(i, j),
//                        Account = wrapper.AccountNumber,
//                        Equity = wrapper.Equity
//                    });

//                    //Portfolio.LivePortfolioTradesList.Add(new Portfolio(symbol, Convert.ToString((int)d.GetValue(i, j)), wrapper.AccountNumber));
//                }
//            }

//            for (var i = 0; i < parameterName.GetLength(0); i++)
//            {
//                for (var j = 0; j < parameterName.GetLength(1); j++)
//                {
//                    var parameter = new MatlabParameter
//                    {
//                        Name = (string) parameterName.GetValue(i, j),
//                        Value = (double) parameterValue.GetValue(i, j),
//                        Account = wrapper.AccountNumber
//                    };
//                    //AddMatlabParameterInDatabase(parameter);
//                    UpdateMatlabParameterInDatabase(parameter);
//                    matlabParametersList.Add(parameter);
//                }
//            }
//            foreach (var mv in matlabValues)
//            {
//                //todo push matlab values on server
//                // AddMatlabValueInDb(mv);
//                Console.WriteLine(mv.Symbol + ": " + mv.Value + ": " + mv.Account + " : " + mv.ValueType);
//            }

//            //WAS USED FOR TESTING PURPOSES
//            //foreach (var bar in UsedMinuteBarList)
//            //{
//            //    InsertUsedBarInDatabase(bar);
//            //}

//            matlabValues.Clear();
//            Portfolio.LivePortfolioTradesList.Clear();
//        }

        
//        /// <summary>
//        ///     Executes the mat lab functions.
//        /// </summary>
//        private void ExecuteMatLabFunctions()
//        {
//            matlab.Execute(MatlabFunction);
//        }

//        /// <summary>
//        ///     Gets the and put work space data.
//        /// </summary>
//        /// <param name="dt">The dt.</param>
//        /// <param name="wrapper">The wrapper.</param>
//        private void GetAndPutWorkSpaceData(DateTime dt, IbClient wrapper)
//        {
//            var x = 1;
           
//                var timeframe = barSettings.Timeframe;
//                var period = barSettings.Period;
//                var symbol = barSettings.Symbol;
//                var value = barSettings.Value;

//                //todo request data from server
//                List<OHLCBar> list = new List<OHLCBar>();
//                //Data.Data.RequestDataFromDb1(
//                //    MyContracts.ContractsExtList.Find(b => b.TickerId.Substring(0, 2) == symbol)
//                //        .TickerId.Substring(0, 2), timeframe, value, period);
//                for (var i = 0; i < 200; i++)
//                {
//                    if (Data.BarCheck(dt, timeframe, list))
//                    {
//                        break;
//                    }
//                    if (i == 199)
//                    {
//                        var msg = "Barcheck failed on " + list[0].Symbol + "/" + list[0].TimeFrame + "||" + DateTime.Now;
//                        Console.WriteLine(msg);
//                        //NetMqMessenger.HandleErrorMsg(8888, msg, wrapper.AccountNumber);
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

//                    //UsedMinuteBarList.Add(new MinuteBar(list[j].Price, list[j].Symbol, list[j].BarDateTime,
//                    //    list[j].TimeFrame));
//                    matlab.Execute(execute);
//                }
//                x++;
//            }

//            matlab.PutWorkspaceData("Equity", "base", new[] {wrapper.Equity});
//            if (matlabParametersList.Count > 0)
//            {
//                foreach (var matlabParameterse in matlabParametersList)
//                {
//                    matlab.PutWorkspaceData(matlabParameterse.Name, "base", matlabParameterse.Value * 1d);
//                }
//            }
//            else
//            {
//                for (var j = 1; j < 11; j++)
//                {
//                    var execute = "p" + j + "=NaN";

//                    matlab.Execute(execute);
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
//        public void StartTrading()
//        {
//            //foreach (var set in AccountSettings.AccountSettingsList)
//            //{
//            //    var wrapper = new IbClient();
//            //    wrapper.ClientSocket.eConnect("127.0.0.1", set.Port, 1, false);
//            //    wrapper.AccountNumber = set.AccountNumber;
//            //    while (wrapper.NextOrderId <= 0)
//            //    {
//            //    }
//            //    wrapper.ClientSocket.reqAccountUpdates(true, wrapper.AccountNumber);
//            //    IbClient.WrapperList.Add(wrapper);
//            //todo get open and live trades from database
//            //NetMqMessanger.SelectOpenOrdersFromDatabase(wrapper.AccountNumber);
//            //NetMqMessanger.SelectLiveTradeFromDatabase(wrapper.AccountNumber);
//            //    SelectMatlabParametersFromDatabase(wrapper.AccountNumber);
//            //}

//            foreach (var symbol in Data.Data.SymbolsList)
//            {
//                MyContracts.GetContractsFromDbOnStartup(symbol);
//            }

//            Thread.Sleep(1000);

//            trade = Db.GetTradeVariableOnStartUp();

//            Execute();

//            while (true)
//            {
//                while (TradingCalendar.IsTradingDay())
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
//                                DateTime.UtcNow.AddHours(-1).ToString("hh") != lastUpdate.AddMinutes(2).ToString("hh"))
//                            {
//                                if (trade)
//                                {
//                                    Thread.Sleep(1200);
//                                    Parallel.Invoke((Execute));

//                                    trade = false;
//                                    lastUpdate = DateTime.UtcNow;
//                                }
//                                else
//                                {
//                                    trade = true;
//                                    lastUpdate = DateTime.UtcNow;
//                                }
//                                Db.InsertTadeInDb(trade);
//                                Thread.Sleep(60000);
//                            }
//                        }
//                    }
//                    else if (DateTime.Now.Minute == 59 && DateTime.Now.Hour == 22)
//                    {
//                        if (trade)
//                        {
//                            Thread.Sleep(1200);
//                            Parallel.Invoke((Execute));

//                            trade = false;
//                            lastUpdate = DateTime.UtcNow;
//                        }
//                        else
//                        {
//                            trade = true;
//                            lastUpdate = DateTime.UtcNow;
//                        }
//                        Db.InsertTadeInDb(trade);
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
//        private void SelectMatlabParametersFromDatabase(string accountNumber)
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
//                                    matlabParametersList.Add(new MatlabParameter
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
//    }
//}