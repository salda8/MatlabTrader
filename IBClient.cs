/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Common;
using ExpressMapper.Extensions;
using IBApi;
using MATLAB_trader.Data.DataType;
using MATLAB_trader.Properties;
using NLog;

namespace MATLAB_trader
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
        private NumberFormatInfo provider= new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        };

        public IbClient(RequestsClient.RequestsClient client)
        {
            Client = client;
            Signal = new EReaderMonitorSignal();
            ClientSocket = new EClientSocket(this, Signal);
            

        }

        public EClientSocket ClientSocket { get; set; }
        public EReaderMonitorSignal Signal { get; set; }
        public int NextOrderId { get; set; }
        public string AccountNumber { get; } = Settings.Default.AccountNumber;
        public decimal Equity { get; set; }

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
                        Client.PushGeneralRequestMesssage(new Equity() {AccountID=Program.AccountID, Value=Equity, UpdateTime=DateTime.Now}, GeneralRequestMessageType.EquityUpdatePush);
                    }



                }
                Client.PushGeneralRequestMesssage(new AccountSummaryUpdate(Program.AccountID, key, Convert.ToDouble(value, provider).ToString(CultureInfo.InvariantCulture)),
                    GeneralRequestMessageType.AccountUpdatePush);
                

            }

        }

        public virtual void UpdatePortfolio(Contract contract, int position, double marketPrice, double marketValue,
            double averageCost, double unrealisedPnl, double realisedPnl, string accountName)
        {
            if (contract.SecType == "FUT")
            {
                logger.Info("UpdatePortfolio. " + contract.Symbol + ", " + contract.SecType + " @ " +
                                  contract.Exchange
                                  + ": Quantity: " + position + ", MarketPrice: " + marketPrice + ", MarketValue: " +
                                  marketValue + ", AveragePrice: " + averageCost
                                  + ", UnrealisedPNL: " + unrealisedPnl + ", RealisedPNL: " + realisedPnl +
                                  ", AccountName: " + accountName + "\n");
                //tf.StartNew(
                //    (() =>
                //        Client.HandlePortfolioUpdate(accountName, contract.Symbol, position,
                //        marketPrice, marketValue, averageCost, realisedPnl, unrealisedPnl)));
            }
        }

       
      
        public virtual void OrderStatus(int orderId, string status, int filled, int remaining, double avgFillPrice,
            int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            logger.Info("OrderStatus. Id: " + orderId + ", Status: " + status + ", Filled" + filled +
                              ", Remaining: " + remaining
                              + ", AverageFillPrice: " + avgFillPrice + ", PermanentId: " + permId + ", ParentId: " + parentId +
                              ", LastFillPrice: " + lastFillPrice + ", ClientId: " + clientId + ", WhyHeld: " + whyHeld +
                              "\n");
            Client.PushGeneralRequestMesssage(ObjectContructorHelper.GetOrderStatusMessage(orderId, status, filled, remaining,
                        avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld), GeneralRequestMessageType.OrderStatusPush);
        }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
           logger.Info("OpenOrder. ID: " + orderId + ", " + contract.Symbol + ", " + contract.SecType + " @ " +
                              contract.Exchange + ": " + order.Action + ", " + order.OrderType + " " +
                              order.TotalQuantity + ", " + orderState.Status + "\n");
            if (lastOrderId == orderId) return; //for some reason IB sends 3 IB messages with status filled
            lastOrderId = orderId;
            Client.PushGeneralRequestMesssage(ObjectContructorHelper.GetOpenOrder(contract, order, orderState), GeneralRequestMessageType.OpenOrderPush);
        }

        public virtual void execDetails(int reqId, Contract contract, Execution execution)
        {
            logger.Info("ExecDetails. " + reqId + " - " + contract.Symbol + ", " + contract.SecType + ", " +
                              contract.Currency + " - " + execution.ExecId + ", " + execution.OrderId + ", " +
                              execution.Shares + "\n");
           Client.PushGeneralRequestMesssage(ObjectContructorHelper.GetExecutionMessage(reqId, contract, execution), GeneralRequestMessageType.ExecutionPush);
        }

        public virtual void commissionReport(CommissionReport commissionReport)
        {
            logger.Info("CommissionReport. " + commissionReport.ExecId + " - " + commissionReport.Commission + " " +
                              commissionReport.Currency + " RPNL " + commissionReport.RealizedPNL + "\n");

           Client.PushGeneralRequestMesssage(commissionReport.Map<CommissionReport, CommissionMessage>(), GeneralRequestMessageType.CommissionPush);

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
            logger.Error("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            // tf.StartNew(() => Client.HandleErrorMsg(errorCode, errorMsg, AccountNumber));
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
            NextOrderId = orderId;
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
        public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue,
                                    double averageCost, double unrealisedPnl, double realisedPnl,
                                    string accountName)
        {
        }
        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice,
                                int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
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

#endregion
    }
    
}