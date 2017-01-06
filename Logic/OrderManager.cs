using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MATLAB_trader.Data;
using MATLAB_trader.Data.DataType;
using QDMS;

namespace MATLAB_trader.Logic
{
    public class OrderManager
    {
        private static readonly TaskFactory Tf = new TaskFactory(TaskScheduler.Default);
        #region DatabaseInteraction

        /// <summary>
        ///     Handles the account update.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void HandleAccountUpdate(string accountName, string key, string value)
        {
            if (value == accountName) return;
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    var doubleValue = Convert.ToDouble(value);

                    switch (key)
                    {
                        case "NetLiquidation":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET NetLiquidation=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";

                            Tf.StartNew(() => HandleEquityUpdate(accountName, doubleValue));
                            Tf.StartNew(() => HandleNetLiquidationAccountUpdate(key, doubleValue, accountName));
                            break;

                        case "CashBalance":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET CashBalance=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;

                        case "DayTradesRemaining":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET DayTradesRemaining=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;

                        case "EquityWithLoanValue":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET EquityWithLoanValue=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;

                        case "InitMarginReq":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET InitMarginReq=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;

                        case "MaintMarginReq":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET MaintMarginReq=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;

                        case "UnrealizedPnL":
                            cmd.CommandText =
                                "UPDATE AccountSummary SET UnrealisedPnL=?doubleValue, Updatetime=?time " +
                                "WHERE AccountNumber=?acc";
                            break;
                    }
                    cmd.Parameters.AddWithValue("?acc", accountName);
                    cmd.Parameters.AddWithValue("?time", DateTime.Now);
                    cmd.Parameters.AddWithValue("?doubleValue", doubleValue);

                    var result = cmd.ExecuteNonQuery();

                    if (result == 0)
                    {
                        HandleAccountInsert(accountName, key, doubleValue);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the live contracts.
        /// </summary>
        /// <param name="result">The result.</param>
        public static void GetLiveContracts(IReadOnlyList<Portfolio> result)
        {
            foreach (var t in result)
            {
                //if (t.Symbol == "EUR")
                //{
                //    Matlab.EurContracts = t.Contracts;
                //}
                //else if (t.Symbol == "GBP")
                //{
                //    Matlab.GbpContracts = t.Contracts;
                //}
                //else if (t.Symbol == "JPY")
                //{
                //    Matlab.JpyContracts = t.Contracts;
                //}
                //else if (t.Symbol == "AUD")
                //{
                //    Matlab.AudContracts = t.Contracts;
                //}
                //else if (t.Symbol == "CAD")
                //{
                //    Matlab.CadContracts = t.Contracts;
                //}
            }
        }

        /// <summary>
        ///     Selects the trades from portfolio table in database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public static void SelectTradesFromDatabase(string accountNumber)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT * FROM Portfolio WHERE Account=?acc";
                    cmd.Parameters.AddWithValue("?acc", accountNumber);

                    var dt = new DataTable(@"PortfolioTable");

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    var result =
                        Enumerable.Select(dt.AsEnumerable(),
                            row => new Portfolio(row[1].ToString(), row[2].ToString(), row[8].ToString()))
                            .ToList();
                    Portfolio.LivePortfolioTradesList.AddRange(result);
                    if (result.Count > 0)
                    {
                        GetLiveContracts(result);
                    }
                }
                //con.Close();
            }
        }

        /// <summary>
        ///     Handles the error MSG.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMsg">The error MSG.</param>
        public static void HandleErrorMsg(int errorCode, string errorMsg, string acc)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO Errors (ErrorCode, ErrorMessage, AccountNumber) VALUES (?ec, ?em, ?acc)";
                    cmd.Parameters.AddWithValue("?acc", acc);
                    cmd.Parameters.AddWithValue("?em", errorMsg);
                    cmd.Parameters.AddWithValue("?ec", errorCode);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Handles the equity update.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="value">The value.</param>
        private static void HandleEquityUpdate(string accountName, double value)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO Equity (AccountNumber, Updatetime, Value) " +
                        "VALUES (?acc, ?time,?value) ";
                    cmd.Parameters.AddWithValue("?acc", accountName);
                    cmd.Parameters.AddWithValue("?time", DateTime.Now);
                    cmd.Parameters.AddWithValue("?value", value);
                    var result = cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Handles the account insert.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private static void HandleAccountInsert(string accountName, string key, double value)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO AccountSummary (AccountNumber, Updatetime) " +
                        "VALUES (?acc, ?time) ";
                    cmd.Parameters.AddWithValue("?acc", accountName);
                    cmd.Parameters.AddWithValue("?time", DateTime.Now);
                    var result = cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Handles the net liquidation account update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="accountName"></param>
        public static void HandleNetLiquidationAccountUpdate(string key, double value, string accountName)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE Portfolio SET NetLiquidation=?value WHERE Account=?acc";
                    cmd.Parameters.AddWithValue("?value", value);
                    cmd.Parameters.AddWithValue("?acc", accountName);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static TradeDirection ConvertFromString(string side) => side=="SLD"? TradeDirection.Long : TradeDirection.Short;

        /// <summary>
        ///     Handles the commission message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleCommissionMessage(CommissionMessage message)
        {
            //todo "creating" HistoryTrade should not be handling this App. Overview app or server. Just add both messages to database.
            if (message.RealizedPnL > 100000000) return;

        }

        
        /// <summary>
        ///     Handles the open order.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleOpenOrder(OpenOrder message)
        {
            

            //_allOpenPositionGrid.ItemsSource = MainWindow.OpenOrderMessageList;
        }

        

        //comes with OpenOrder
        /// <summary>
        ///     Handles the order status.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleOrderStatus(OrderStatusMessage message)
        {
           
        }
        

        /// <summary>
        ///     Handles the execution message.
        ///     todo not sure if this should be here and not on server
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleExecutionMessage(ExecutionMessage message)
        {
            
           
        }

       /// <summary>
        ///     Handles the portfolio update.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="s">The symbol.</param>
        /// <param name="position">The position.</param>
        /// <param name="marketPrice">The market price.</param>
        /// <param name="marketValue">The market value.</param>
        /// <param name="averageCost">The average cost.</param>
        /// <param name="realisedPnl">The realised PNL.</param>
        /// <param name="unrealisedPnl">The unrealised PNL.</param>
        public static void HandlePortfolioInsert(string accountName, string s, int position, double marketPrice,
            double marketValue, double averageCost, double realisedPnl, double unrealisedPnl)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT Portfolio (Symbol,Quantity,MarketPrice,MarketValue,AveragePrice,UnrealizedPnL,RealizedPnL, Account,Updatetime) " +
                        "VALUES (?symbol,?pos,?mp,?mv,?ac,?upnl,?rpnl, ?acc,?time)";
                    cmd.Parameters.AddWithValue("?symbol", s);
                    cmd.Parameters.AddWithValue("?pos", position);
                    cmd.Parameters.AddWithValue("?mp", marketPrice);
                    cmd.Parameters.AddWithValue("?mv", marketValue);
                    cmd.Parameters.AddWithValue("?ac", averageCost);
                    cmd.Parameters.AddWithValue("?upnl", unrealisedPnl);
                    cmd.Parameters.AddWithValue("?rpnl", realisedPnl);
                    cmd.Parameters.AddWithValue("?acc", accountName);
                    cmd.Parameters.AddWithValue("?time", DateTime.Now);

                    var result = cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Handles the portfolio update.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="position">The position.</param>
        /// <param name="marketPrice">The market price.</param>
        /// <param name="marketValue">The market value.</param>
        /// <param name="averageCost">The average cost.</param>
        /// <param name="realisedPnl">The realised PNL.</param>
        /// <param name="unrealisedPnl">The unrealised PNL.</param>
        public static void HandlePortfolioUpdate(string accountName, string symbol, int position, double marketPrice,
            double marketValue, double averageCost, double realisedPnl, double unrealisedPnl)
        {
            Portfolio.LivePortfolioTradesList.Add(new Portfolio(symbol, Convert.ToString(position), accountName));
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE Portfolio SET Symbol=?symbol,Quantity=?pos,MarketPrice=?mp,MarketValue=?mv,AveragePrice=?ac,UnrealizedPnL=?upnl,RealizedPnL=?rpnl,Updatetime=?time " +
                        "WHERE Symbol=?symbol AND Account=?acc";
                    cmd.Parameters.AddWithValue("?symbol", symbol);
                    cmd.Parameters.AddWithValue("?pos", position);
                    cmd.Parameters.AddWithValue("?mp", marketPrice);
                    cmd.Parameters.AddWithValue("?mv", marketValue);
                    cmd.Parameters.AddWithValue("?ac", averageCost);
                    cmd.Parameters.AddWithValue("?upnl", unrealisedPnl);
                    cmd.Parameters.AddWithValue("?rpnl", realisedPnl);
                    cmd.Parameters.AddWithValue("?acc", accountName);
                    cmd.Parameters.AddWithValue("?Time", DateTime.Now);
                    try
                    {
                        var rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            HandlePortfolioInsert(accountName, symbol, position, marketPrice, marketValue, averageCost,
                                realisedPnl,
                                unrealisedPnl);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Looks OBSOLETE
        ///     Selects the open orders from database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        //public static void SelectOpenOrdersFromDatabase(string accountNumber)
        //{
        //    using (var con = Db.OpenConnection())
        //    {
        //        using (var cmd = con.CreateCommand())
        //        {
        //            cmd.CommandText =
        //                "SELECT * FROM OpenOrders WHERE AccountNumber=?acc";
        //            cmd.Parameters.AddWithValue("?acc", "DU" + accountNumber);

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    OpenOrderMessageList.Add(new OpenOrder
        //                    {
        //                        Account = reader.GetString(6),
        //                        PermanentId = reader.GetInt32(1),
        //                        ContractSymbol = reader.GetString(2),
        //                        Status = reader.GetString(3),
        //                        LimitPrice = reader.GetDouble(4),
        //                        Quantity = reader.GetInt16(5),
        //                        Side = reader.GetString(7),
        //                        Type = reader.GetString(9),
        //                        Tif = reader.GetString(10)
        //                    });
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// LOOKS OBSOLETE
        ///     Selects the live trade from database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        //public static void SelectLiveTradeFromDatabase(string accountNumber)
        //{
        //    using (var con = Db.OpenConnection())
        //    {
        //        using (var cmd = con.CreateCommand())
        //        {
        //            cmd.CommandText =
        //                "SELECT * FROM livetrades WHERE AccountNumber=?acc";
        //            cmd.Parameters.AddWithValue("?acc", "DU" + accountNumber);

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    LiveTrades.Add(new LiveTrade(reader.GetInt16(1), reader.GetString(3),
        //                        reader.GetInt16(6), reader.GetString(2).Substring(2, 5),
        //                        reader.GetString(2).Substring(0, 2), reader.GetInt16(4), reader.GetString(5),
        //                        reader.GetDouble(7), reader.GetDateTime(10)));
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}