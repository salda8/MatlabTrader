/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using Common.EntityModels;
using Common.Enums;
using ExpressMapper.Extensions;
using IBApi;
using NLog;
using StrategyTrader.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTrader
{
    public class IbClient : EWrapper
    {
        public RequestsClient.RequestsClient Client { get; }
        public static List<IbClient> WrapperList = new List<IbClient>();
        public static bool Firstime = true;
        public static double LastPrice;
        public static DateTimeOffset Time;
        private readonly TaskFactory tf = new TaskFactory();
        private int lastOrderId;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ReaderWriterLockSlim nextOrderIdLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim lastOrderIdLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim lastOrderStatusIdLock = new ReaderWriterLockSlim();

        private readonly NumberFormatInfo provider = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        };

        private bool openOrderEndEnded = false;
        private int nextOrderId;
        private int lastOrderStatusId;
        private string lastOrderStatus;
        private string lastOrderStatusStatus;

        public IbClient(RequestsClient.RequestsClient client)
        {
            Client = client;
            Signal = new EReaderMonitorSignal();
            ClientSocket = new EClientSocket(this, Signal);
        }

        public EClientSocket ClientSocket { get; set; }
        public EReaderMonitorSignal Signal { get; set; }

        public int NextOrderId
        {
            get
            {
                nextOrderIdLock.TryEnterUpgradeableReadLock(-1);
                try
                {
                    nextOrderId = nextOrderId + 1;
                    return nextOrderId;
                }
                finally
                {
                    if (nextOrderIdLock.IsUpgradeableReadLockHeld)
                    {
                        nextOrderIdLock.ExitUpgradeableReadLock();
                    }
                }
            }
        }

        public string AccountNumber { get; } = Settings.Default.AccountNumber;
        public decimal Equity { get; set; }
        public List<Order> OpenOrders { get; set; } = new List<Order>(10);
        public double LiveOrderQuantity { get; set; }

        public bool ShouldCollectOpenOrders { get; set; }

        public bool OpenOrderEndEnded
        {
            get { return openOrderEndEnded; }
            set { openOrderEndEnded = value; }
        }

        public virtual void updateAccountValue(string key, string value, string currency, string accountName)
        {
            if ((key == "NetLiquidation" || key == "CashBalance" || key == "DayTradesRemaining" ||
                 key == "EquityWithLoanValue" || key == "InitMarginReq" || key == "MaintMarginReq"
                 || key == "UnrealizedPnL") && currency == "USD")
            {
                if (key == "NetLiquidation" || key == "UnrealizedPnL")
                {
                    logger.Info("UpdateAccountValue. Key: " + key + ", Value: " + value + ", Currency: " +
                                currency +
                                ", AccountName: " + accountName + "\n");
                    if (key == "NetLiquidation")
                    {
                        Equity = Convert.ToDecimal(value, provider);
                        Client.PushPushMessageType(new Equity() { AccountID = Settings.Default.AccountID, Value = Equity, UpdateTime = DateTime.Now }, PushMessageType.EquityUpdatePush);
                    }
                }
                Client.PushPushMessageType(new AccountSummaryUpdate(Settings.Default.AccountID, key, Convert.ToDouble(value, provider).ToString(CultureInfo.InvariantCulture)),
                    PushMessageType.AccountUpdatePush);
            }
        }

        public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue,
                                     double averageCost, double unrealisedPnl, double realisedPnl,
                                     string accountName)
        {

            logger.Info("UpdatePortfolio. " + contract.Symbol + " " + contract.LocalSymbol + ", " + contract.SecType + " @ " +
                                contract.Exchange
                                + ": Quantity: " + position + ", MarketPrice: " + marketPrice + ", MarketValue: " +
                                marketValue + ", AveragePrice: " + averageCost
                                + ", UnrealisedPNL: " + unrealisedPnl + ", RealisedPNL: " + realisedPnl +
                                ", AccountName: " + accountName + "\n");
            if (position != 0)
            {
                Client.PushPushMessageType(
                    ObjectConstructorHelper.GetLiveTrade(position, marketPrice, averageCost,
                        unrealisedPnl, realisedPnl), PushMessageType.LiveTradePush);
                LiveOrderQuantity = position;
            }
        }

        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice,
                                int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            lastOrderStatusIdLock.TryEnterUpgradeableReadLock(-1);
            try
            {
                if (lastOrderStatusId == orderId && lastOrderStatusStatus == status) return; //for some reason IB sends sometime more than 1 IB messages with same status and id
                lastOrderStatusId = orderId;
                lastOrderStatusStatus = status;
            }
            finally
            {
                if (lastOrderStatusIdLock.IsUpgradeableReadLockHeld)
                {
                    lastOrderStatusIdLock.ExitUpgradeableReadLock();
                }
            }

            logger.Info("OrderStatus. Id: " + orderId + ", Status: " + status + ", AverageFillPrice: " + avgFillPrice + ", PermanentId: " + permId

                             );
            Client.PushPushMessageType(ObjectConstructorHelper.GetOrderStatusMessage(orderId, status, Convert.ToInt32(filled), Convert.ToInt32(remaining),
                        avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld), PushMessageType.OrderStatusPush);
        }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            if (OpenOrderEndEnded)
            {
                lastOrderIdLock.TryEnterUpgradeableReadLock(-1);
                try
                {
                    if ((lastOrderId == orderId && lastOrderStatus == orderState.Status)) return; //for some reason IB sends sometime more than 1 IB messages with same status and id
                    lastOrderId = orderId;
                    lastOrderStatus = orderState.Status;
                }
                finally
                {
                    if (lastOrderIdLock.IsUpgradeableReadLockHeld)
                    {
                        lastOrderIdLock.ExitUpgradeableReadLock();
                    }
                }

                logger.Info("OpenOrder. ID: " + orderId + ", " + contract.LocalSymbol + ", " + contract.SecType + " @ " +
                            contract.Exchange + ": " + order.Action + ", " + order.OrderType + " " +
                            order.TotalQuantity + ", " + orderState.Status + "\n");

                Client.PushPushMessageType(ObjectConstructorHelper.GetOpenOrder(contract, order, orderState),
                    PushMessageType.OpenOrderPush);
            }
            else
            {
                if (ShouldCollectOpenOrders)
                {
                    OpenOrders.Add(order);
                }
            }
        }

        public virtual void execDetails(int reqId, Contract contract, Execution execution)
        {
            logger.Info("ExecDetails. " + contract.Symbol + ", " + execution.Side +
                             " - " + execution.ExecId + ", " + execution.OrderId + ", " +
                              execution.Shares + "\n");
            Client.PushPushMessageType(ObjectConstructorHelper.GetExecutionMessage(reqId, contract, execution), PushMessageType.ExecutionPush);
        }

        public virtual void commissionReport(CommissionReport commissionReport)
        {
            logger.Info("CommissionReport. " + commissionReport.ExecId + " - " + commissionReport.Commission + " " +
                              commissionReport.Currency + " RPNL " + commissionReport.RealizedPNL + "\n");

            Client.PushPushMessageType(commissionReport.Map<CommissionReport, CommissionMessage>(), PushMessageType.CommissionPush);
        }

        public virtual void error(Exception e)
        {
            logger.Error("Exception thrown: " + e);
            throw e;
        }

        public virtual void error(string str)
        {
            logger.Error("Error: " + str + "\n");
        }

        public virtual void error(int id, int errorCode, string errorMsg)
        {
            if (errorCode == 201 && errorMsg.Contains("15 orders")) //too many orders
            {
                ClientSocket.reqGlobalCancel();
            }
            else if (errorCode == 202)//errro happens when reqGlobalCancel is called
            {
                return;
            }
           else if (errorCode == 161)//order is not in cancellable state, happens when reqGlobalCancel is called
            {
                return;
            }
            else if (errorCode == 2107)//historical data from IB. We are taking historical data from server anyway.
            {
                return;
            }

            logger.Error("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            
        }

        #region UnusedIbApiMethods

        public virtual void realtimeBar(int reqId, long time, double open, double high, double low, double close,
            long volume, double wap, int count)
        {
            logger.Info("RealTimeBars. " + reqId + " - Time: " + UnixTimeStampToDateTime(Convert.ToDouble(time)) +
                              ", Open: " + open + ", High: " + high + ", Low: " + low + ", Close: " + close +
                              ", Volume: " + volume + ", Count: " + count + ", WAP: " + wap + "\n");
        }

        public virtual void historicalData(int reqId, string date, double open, double high, double low, double close,
            int volume, int count, double wap, bool hasGaps)
        {
        }

        public virtual void connectionClosed()
        {
            logger.Info("Connection closed.\n");
        }

        public virtual void currentTime(long time)
        {
            logger.Info("Current Time: " + UnixTimeStampToDateTime(Convert.ToDouble(time)) + "\n");
        }

        public virtual void tickPrice(int tickerId, int field, double price, int canAutoExecute)
        {
            if (field == 4)
            {
            }
        }

        public virtual void tickSize(int tickerId, int field, int size)
        {
            //logger.Info("Tick Size. Ticker Id:" + tickerId + ", Field: " + field + ", Size: " + size+"\n");
        }

        public virtual void tickString(int tickerId, int tickType, string value)
        {
            /*logger.Info("Tick string. Ticker Id:" + tickerId + ", Type: " + tickType + ", Value: " + UnixTimeStampToDateTime(Convert.ToDouble(value)));
            Console.Write(DateTime.Now.ToString("mm:ss.fff"));
            logger.Info("***************************"+"\n");*/
        }

        public virtual void tickGeneric(int tickerId, int field, double value)
        {
        }

        public virtual void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints,
            double impliedFuture, int holdDays, string futureExpiry, double dividendImpact, double dividendsToExpiry)
        {
        }

        public virtual void tickSnapshotEnd(int tickerId)
        {
            logger.Info("TickSnapshotEnd: " + tickerId + "\n");
        }

        public virtual void nextValidId(int orderId)
        {
            logger.Info("Next Valid Id: " + orderId + "\n");
            nextOrderId = orderId;
        }

        public virtual void deltaNeutralValidation(int reqId, UnderComp underComp)
        {
        }

        public virtual void updateAccountTime(string timestamp)
        {
            //logger.Info("UpdateAccountTime. Time: " + timestamp + "\n");
        }

        public virtual void accountDownloadEnd(string account)
        {
            logger.Info("Account download finished: " + account + "\n");
        }

        public virtual void managedAccounts(string accountsList)
        {
            Settings.Default.AccountNumber = accountsList;
            Settings.Default.Save();
            logger.Info("Account list: " + accountsList + "\n");
        }

        public virtual void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta,
            double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
        }

        public virtual void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            logger.Info("Acct Summary. ReqId: " + reqId + ", Acct: " + account + ", Tag: " + tag + ", Value: " +
                              value + ", Currency: " + currency + "\n");
        }

        public virtual void accountSummaryEnd(int reqId)
        {
            logger.Info("AccountSummaryEnd. Req Id: " + reqId + "\n");
        }

        public virtual void openOrderEnd()
        {
            OpenOrderEndEnded = true;
            logger.Info("OpenOrderEnd");
        }

        public virtual void contractDetails(int reqId, ContractDetails contractDetails)
        {
            logger.Info("ContractDetails. ReqId: " + reqId + " - " + contractDetails.Summary.Symbol + ", " +
                              contractDetails.Summary.SecType + ", ConId: " + contractDetails.Summary.ConId + " @ " +
                              contractDetails.Summary.Exchange + "\n");
        }

        public virtual void contractDetailsEnd(int reqId)
        {
            logger.Info("ContractDetailsEnd. " + reqId + "\n");
        }

        public virtual void execDetailsEnd(int reqId)
        {
            logger.Info("ExecDetailsEnd. " + reqId + "\n");
        }

        public virtual void fundamentalData(int reqId, string data)
        {
            logger.Info("FundamentalData. " + reqId + "" + data + "\n");
        }

        public virtual void marketDataType(int reqId, int marketDataType)
        {
            logger.Info("MarketDataType. " + reqId + ", Type: " + marketDataType + "\n");
        }

        public virtual void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            logger.Info("UpdateMarketDepth. " + tickerId + " - Quantity: " + position + ", Operation: " +
                              operation + ", Side: " + side + ", Price: " + price + ", Size" + size + "\n");
        }

        public virtual void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side,
            double price, int size)
        {
            logger.Info("UpdateMarketDepthL2. " + tickerId + " - Quantity: " + position + ", Operation: " +
                              operation + ", Side: " + side + ", Price: " + price + ", Size" + size + "\n");
        }

        public virtual void updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
        {
            logger.Info("News Bulletins. " + msgId + " - Type: " + msgType + ", Message: " + message +
                              ", Exchange of Origin: " + origExchange + "\n");
        }

        public virtual void Position(string account, Contract contract, int pos, double avgCost)
        {
            logger.Info("Quantity. " + account + " - Symbol: " + contract.Symbol + ", SecType: " +
                              contract.SecType + ", Currency: " + contract.Currency + ", Quantity: " + pos +
                              ", Avg cost: " + avgCost + "\n");
        }

        public virtual void positionEnd()
        {
            logger.Info("PositionEnd \n");
        }

        public virtual void scannerParameters(string xml)
        {
            logger.Info("ScannerParameters. " + xml + "\n");
        }

        public virtual void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance,
            string benchmark, string projection, string legsStr)
        {
            logger.Info("ScannerData. " + reqId + " - Rank: " + rank + ", Symbol: " +
                              contractDetails.Summary.Symbol + ", SecType: " + contractDetails.Summary.SecType +
                              ", Currency: " + contractDetails.Summary.Currency
                              + ", Distance: " + distance + ", Benchmark: " + benchmark + ", Projection: " + projection +
                              ", Legs String: " + legsStr + "\n");
        }

        public virtual void scannerDataEnd(int reqId)
        {
            logger.Info("ScannerDataEnd. " + reqId + "\n");
        }

        public virtual void receiveFA(int faDataType, string faXmlData)
        {
            logger.Info("Receing FA: " + faDataType + " - " + faXmlData + "\n");
        }

        public virtual void bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            logger.Info("Bond. Symbol " + contractDetails.Summary.Symbol + ", " + contractDetails.Summary);
        }

        public virtual void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            logger.Info("Historical data end - " + reqId + " from " + startDate + " to " + endDate + " TIME:" +
                              DateTime.UtcNow.ToString("ss.ffff"));
        }

        public virtual void verifyMessageAPI(string apiData)
        {
            logger.Info("verifyMessageAPI: " + apiData);
        }

        public virtual void verifyCompleted(bool isSuccessful, string errorText)
        {
            logger.Info("verifyCompleted. IsSuccessfule: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            logger.Info("verifyAndAuthMessageAPI: " + apiData + " " + xyzChallenge);
        }

        public virtual void verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            logger.Info("verifyAndAuthCompleted. IsSuccessful: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void displayGroupList(int reqId, string groups)
        {
            logger.Info("DisplayGroupList. Request: " + reqId + ", Groups" + groups);
        }

        public virtual void displayGroupUpdated(int reqId, string contractInfo)
        {
            logger.Info("displayGroupUpdated. Request: " + reqId + ", ContractInfo: " + contractInfo);
        }

        public static string UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var userTime = TimeZoneInfo.ConvertTimeFromUtc(dtDateTime, easternZone);
            return userTime.ToString("hh:mm:ss.fff");
        }

        public void position(string account, Contract contract, double pos, double avgCost)
        {
        }

        public void connectAck()
        {
        }

        public void positionMulti(int requestId, string account, string modelCode, Contract contract, double pos,
                                  double avgCost)
        {
        }

        public void positionMultiEnd(int requestId)
        {
        }

        public void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value,
                                       string currency)
        {
        }

        public void accountUpdateMultiEnd(int requestId)
        {
        }

        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId,
                                                      string tradingClass, string multiplier, HashSet<string> expirations,
                                                      HashSet<double> strikes)
        {
        }

        public void securityDefinitionOptionParameterEnd(int reqId)
        {
        }

        public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
        }

        #endregion UnusedIbApiMethods
    }
}