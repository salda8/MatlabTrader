using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MATLAB_trader.Data;
using MATLAB_trader.Data.DataType;

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
                if (t.Symbol == "EUR")
                {
                    Matlab.EurContracts = t.Contracts;
                }
                else if (t.Symbol == "GBP")
                {
                    Matlab.GbpContracts = t.Contracts;
                }
                else if (t.Symbol == "JPY")
                {
                    Matlab.JpyContracts = t.Contracts;
                }
                else if (t.Symbol == "AUD")
                {
                    Matlab.AudContracts = t.Contracts;
                }
                else if (t.Symbol == "CAD")
                {
                    Matlab.CadContracts = t.Contracts;
                }
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

        /// <summary>
        ///     Handles the commission message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleCommissionMessage(CommissionMessage message)
        {
            if (message.RealizedPnL > 100000000) return;

            var list = new List<CommissionMessage> {message};
            var item = ExecutionMessage.ExecutionMessageList.LastOrDefault(x => x.ExecutionId == message.ExecutionId);

            if (item != null)
            {
                item.Side = item.Side == "SLD" ? "BOT" : "SLD";

                var join = (from s1 in ExecutionMessage.ExecutionMessageList
                    join s2 in list
                        on s1.ExecutionId equals s2.ExecutionId
                    select new HistoricalTrade
                        (
                        s1.AccountNumber,
                        s1.ExecutionId,
                        s1.Time,
                        s1.Side,
                        s1.Qty,
                        s1.ContractSymbol + " " + s1.ContractSecType,
                        s1.Price,
                        s2.Commission,
                        s2.RealizedPnL
                        )).ToList();
                //if (@join.Count != 1)
                //{
                //    return;
                //}
                HistoricalTrade.HistoricalTradeList.Add(@join[0]);
                HistoricalTradeListInsertQuery(@join[0]);
            }
            else
            {
                Console.WriteLine("Can not find execution message with id:" + message.ExecutionId + " || " +
                                  DateTime.Now);
            }
        }

        /// <summary>
        ///     Historical trades insert query.
        /// </summary>
        /// <param name="historicalTrade">The historical trade.</param>
        private static void HistoricalTradeListInsertQuery(HistoricalTrade historicalTrade)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO HistoricalTrades (ExecutionId, Qty, Side, Description, Price, Commission, RealizedPnL, ExecutionTime, AccountNumber) " +
                        "VALUES(@permId,@AccountNumber,@OrderID,@ContractSymbol,@Qty,@Side,@AvgFillPrice,@Time,@Acc)";
                    cmd.Parameters.AddWithValue("@permId", historicalTrade.ExecutionId);
                    cmd.Parameters.AddWithValue("@AccountNumber", historicalTrade.Quantity);
                    cmd.Parameters.AddWithValue("@OrderID", historicalTrade.Side);
                    cmd.Parameters.AddWithValue("@ContractSymbol", historicalTrade.Description);
                    cmd.Parameters.AddWithValue("@Qty", historicalTrade.Price);
                    cmd.Parameters.AddWithValue("@Side", historicalTrade.Commission);
                    cmd.Parameters.AddWithValue("@AvgFillPrice", historicalTrade.RealizedPnL);
                    cmd.Parameters.AddWithValue("@Time", historicalTrade.ExecTime);
                    cmd.Parameters.AddWithValue("@Acc", historicalTrade.Account);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Handles the open order.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleOpenOrder(OpenOrderMessage message)
        {
            if (message.OrderState == "Submitted")
            {
                OpenOrderMessage.OpenOrderMessageList.Add(message);
                OpenOrderListInsertQuery(message);
            }
            else if (message.OrderState == "Cancelled")
            {
                CheckSubmittedOrders(message);
            }
            else if (message.OrderState == "Filled")
            {
                CheckSubmittedOrders(message);
            }

            //_allOpenPositionGrid.ItemsSource = MainWindow.OpenOrderMessageList;
        }

        /// <summary>
        ///     Opens the order list insert query.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void OpenOrderListInsertQuery(OpenOrderMessage message)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO OpenOrders (PermId, AccountNumber, ContractSymbol, Status, LimitPrice, TotalQuantity) " +
                        "VALUES(@permId,@AccountNumber,@ContractSymbol,@Qty,@Side,@AvgFillPrice)";
                    cmd.Parameters.AddWithValue("@permId", message.PermId);
                    cmd.Parameters.AddWithValue("@AccountNumber", message.Account);
                    cmd.Parameters.AddWithValue("@ContractSymbol", message.ContractSymbol);
                    cmd.Parameters.AddWithValue("@Qty", message.OrderState);
                    cmd.Parameters.AddWithValue("@Side", message.LimitPrice);
                    cmd.Parameters.AddWithValue("@AvgFillPrice", message.Qty);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //comes with OpenOrder
        /// <summary>
        ///     Handles the order status.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleOrderStatus(OrderStatusMessage message)
        {
            if (message.Status.ToUpper() == "CANCELLED" || message.Status.ToUpper() == "FILLED")
            {
                CheckSubmittedOrders(message);
            }
        }

        /// <summary>
        ///     Checks the submitted orders.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void CheckSubmittedOrders(OrderStatusMessage message)
        {
            var item = OpenOrderMessage.OpenOrderMessageList.FindIndex(x => x.PermId == message.PermId);
            if (item == -1) return;
            OpenOrderMessage.OpenOrderMessageList.RemoveAt(item);
            OpenOrderListDeleteQuery(OpenOrderMessage.OpenOrderMessageList[item].PermId);
        }

        /// <summary>
        ///     Checks the submitted orders.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void CheckSubmittedOrders(OpenOrderMessage message)
        {
            var item = OpenOrderMessage.OpenOrderMessageList.FindIndex(x => x.PermId == message.PermId);
            if (item == -1) return;
            OpenOrderMessage.OpenOrderMessageList.RemoveAt(item);
            OpenOrderListDeleteQuery(OpenOrderMessage.OpenOrderMessageList[item].PermId);
        }

        /// <summary>
        ///     delete query.
        /// </summary>
        /// <param name="permId">The perm identifier.</param>
        private static void OpenOrderListDeleteQuery(int permId)
        {
            //var con = Db.OpenConnection();
            //var cmd = con.CreateCommand();
            //cmd.CommandText =
            //    "DELETE FROM OpenOrder" + "WHERE PermID=@PermID";
            //cmd.Parameters.AddWithValue("@PermID", permId);
            //cmd.ExecuteNonQuery();
            //con.Close();
        }

        /// <summary>
        ///     Handles the execution message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void HandleExecutionMessage(ExecutionMessage message)
        {
            ExecutionMessage.ExecutionMessageList.Add(message);
            if (ItIsFirstTradeOnAccount(message))
            {
                var liveTrade = new LiveTrade(message.PermId, message.AccountNumber, message.OrderId,
                    message.ContractSecType, message.ContractSymbol, message.Qty, message.Side, message.Price,
                    message.Time);
                LiveTrade.LiveTradeListCollection.Add(liveTrade);
                LiveTradeInsertQuery(liveTrade);
            }
            else if (ItIsSameSideTrade(message))
            {
                var item =
                    LiveTrade.LiveTradeListCollection.LastOrDefault(
                        x => x.AccountNumber == message.AccountNumber && x.ContractSymbol == message.ContractSymbol);
                var newFillPrice = (item.AvgFillPrice*item.Qty + message.Price*message.Qty)/(item.Qty + message.Qty);
                var newQty = item.Qty + message.Qty;

                var liveTrade = new LiveTrade(message.PermId, message.AccountNumber, message.OrderId,
                    message.ContractSecType, message.ContractSymbol, newQty, message.Side, newFillPrice, message.Time);
                LiveTrade.LiveTradeListCollection.Add(liveTrade);
                //LiveTradeUpdateQuery(liveTrade);
                LiveTradeListDeleteQuery(message);
                LiveTradeInsertQuery(liveTrade);
                LiveTrade.LiveTradeListCollection.Remove(item);
            }
            else //different side trade
            {
                var item =
                    LiveTrade.LiveTradeListCollection.LastOrDefault(
                        x => x.AccountNumber == message.AccountNumber && x.ContractSymbol == message.ContractSymbol);

                if (message.Qty > item.Qty)
                {
                    var newFillPrice = (item.AvgFillPrice*item.Qty - message.Price*message.Qty)/(item.Qty - message.Qty);
                    var newQty = message.Qty - item.Qty;
                    var liveTrade = new LiveTrade(message.PermId, message.AccountNumber, message.OrderId,
                        message.ContractSecType, message.ContractSymbol, newQty, message.Side, newFillPrice,
                        message.Time);
                    LiveTrade.LiveTradeListCollection.Add(liveTrade);
                    LiveTradeListDeleteQuery(message);
                    LiveTradeInsertQuery(liveTrade);
                }
                else
                {
                    LiveTradeListDeleteQuery(message);
                }
                LiveTrade.LiveTradeListCollection.Remove(item);
            }

            // Insert
        }

        /// <summary>
        ///     Update live trade in db.
        /// </summary>
        /// <param name="liveTrade">The live trade.</param>
        private static void LiveTradeUpdateQuery(LiveTrade liveTrade)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE livetrades SET PermId=?permId,Qty=?qty, Side=?side, AvgFillPrice=?avgFillPrice, Time=?time WHERE AccountNumber=?accountNumber AND ContractSymbol=?contractSymbol";
                    cmd.Parameters.AddWithValue("?permId", liveTrade.PermId);
                    cmd.Parameters.AddWithValue("?accountNumber", liveTrade.AccountNumber);
                    cmd.Parameters.AddWithValue("?orderID", liveTrade.OrderId);
                    cmd.Parameters.AddWithValue("?contractSymbol", liveTrade.ContractSymbol);

                    cmd.Parameters.AddWithValue("?qty", liveTrade.Qty);
                    cmd.Parameters.AddWithValue("?side", liveTrade.Side);
                    cmd.Parameters.AddWithValue("?avgFillPrice", liveTrade.AvgFillPrice);
                    cmd.Parameters.AddWithValue("?time", liveTrade.Time);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Inserts live trade in db.
        /// </summary>
        /// <param name="liveTrade">The live trade.</param>
        private static void LiveTradeInsertQuery(LiveTrade liveTrade)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO LiveTrades (PermId, AccountNumber, OrderID, ContractSymbol, Qty, Side, AvgFillPrice, Time) " +
                        "VALUES(@permId,@AccountNumber,@OrderID,@ContractSymbol,@Qty,@Side,@AvgFillPrice,@Time)";
                    cmd.Parameters.AddWithValue("@permId", liveTrade.PermId);
                    cmd.Parameters.AddWithValue("@AccountNumber", liveTrade.AccountNumber);
                    cmd.Parameters.AddWithValue("@OrderID", liveTrade.OrderId);
                    cmd.Parameters.AddWithValue("@ContractSymbol", liveTrade.ContractSymbol);
                    cmd.Parameters.AddWithValue("@Qty", liveTrade.Qty);
                    cmd.Parameters.AddWithValue("@Side", liveTrade.Side);
                    cmd.Parameters.AddWithValue("@AvgFillPrice", liveTrade.AvgFillPrice);
                    cmd.Parameters.AddWithValue("@Time", liveTrade.Time);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Check if the same side trade exist.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static
            bool ItIsSameSideTrade(ExecutionMessage message)
        {
            var itemfound = LiveTrade.LiveTradeListCollection.Any(
                x =>
                    x.AccountNumber == message.AccountNumber && x.Side == message.Side &&
                    x.ContractSymbol == message.ContractSymbol);
            return itemfound;
        }

        /// <summary>
        ///     Check if on same account and same contract exists any trade.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static bool ItIsFirstTradeOnAccount(ExecutionMessage message)
        {
            var itemfound =
                LiveTrade.LiveTradeListCollection.Any(
                    x => x.AccountNumber == message.AccountNumber && x.ContractSymbol == message.ContractSymbol);
            return !itemfound;
        }

        /// <summary>
        ///     Lives the trade list delete query.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void LiveTradeListDeleteQuery(ExecutionMessage message)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "DELETE FROM LiveTrades WHERE AccountNumber = ?AccountNumber AND ContractSymbol=?symbol";
                    cmd.Parameters.AddWithValue("?AccountNumber", message.AccountNumber);
                    cmd.Parameters.AddWithValue("?symbol", message.ContractSymbol);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //private void OpenOrderCheck(ExecutionMessage message)
        //{
        //    //var item =
        //    //    MainWindow.OrderStatusOpenOrderJoinList.SingleOrDefault(x => x.PermID == message.PermId);
        //    //if (item == null) return;
        //    //MainWindow.OrderStatusOpenOrderJoinList.Remove(item);
        //}

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
                        "INSERT Portfolio (Symbol,Position,MarketPrice,MarketValue,AverageCost,UnrealizedPnL,RealizedPnL, Account,Updatetime) " +
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
                        "UPDATE Portfolio SET Symbol=?symbol,Position=?pos,MarketPrice=?mp,MarketValue=?mv,AverageCost=?ac,UnrealizedPnL=?upnl,RealizedPnL=?rpnl,Updatetime=?time " +
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
        ///     Selects the open orders from database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public static void SelectOpenOrdersFromDatabase(string accountNumber)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT * FROM OpenOrders WHERE AccountNumber=?acc";
                    cmd.Parameters.AddWithValue("?acc", "DU" + accountNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OpenOrderMessage.OpenOrderMessageList.Add(new OpenOrderMessage
                            {
                                Account = reader.GetString(6),
                                PermId = reader.GetInt32(1),
                                ContractSymbol = reader.GetString(2),
                                OrderState = reader.GetString(3),
                                LimitPrice = reader.GetDouble(4),
                                Qty = reader.GetInt16(5),
                                Side = reader.GetString(7),
                                Type = reader.GetString(9),
                                Tif = reader.GetString(10)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Selects the live trade from database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public static void SelectLiveTradeFromDatabase(string accountNumber)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT * FROM livetrades WHERE AccountNumber=?acc";
                    cmd.Parameters.AddWithValue("?acc", "DU" + accountNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LiveTrade.LiveTradeListCollection.Add(new LiveTrade(reader.GetInt16(1), reader.GetString(3),
                                reader.GetInt16(6), reader.GetString(2).Substring(2, 5),
                                reader.GetString(2).Substring(0, 2), reader.GetInt16(4), reader.GetString(5),
                                reader.GetDouble(7), reader.GetDateTime(10)));
                        }
                    }
                }
            }
        }

        #endregion
    }
}