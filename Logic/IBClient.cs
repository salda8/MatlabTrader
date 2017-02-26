/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using IBApi;
using MATLAB_trader.Data.DataType;
using QDMS;

namespace MATLAB_trader.Logic
{
    public class IbClient : EWrapper
    {
        private readonly NetMqMessenger netMqMessanger;
        public static List<IbClient> WrapperList = new List<IbClient>();
        public static bool Firstime = true;
        public static double LastPrice;
        public static DateTimeOffset Time;
        private readonly TaskFactory tf = new TaskFactory();
        private EReaderMonitorSignal signal;

        public IbClient(NetMqMessenger netMqMessanger)
        {
            this.netMqMessanger = netMqMessanger;
            Signal = new EReaderMonitorSignal();
            ClientSocket = new EClientSocket(this, Signal);
            

        }

        public EClientSocket ClientSocket { get; set; }

        public EReaderMonitorSignal Signal
        {
            get { return signal; }
            set { signal = value; }
        }

        
        public int NextOrderId { get; set; }
        public string AccountNumber { get; set; }
        public double Equity { get; set; }

        public virtual void error(Exception e)
        {
            Console.WriteLine("Exception thrown: " + e);
            throw e;
        }

        public virtual void error(string str)
        {
            Console.WriteLine("Error: " + str + "\n");
        }

        public virtual void error(int id, int errorCode, string errorMsg)
        {
            Console.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
           // tf.StartNew(() => NetMqMessanger.HandleErrorMsg(errorCode, errorMsg, AccountNumber));
        }

        public virtual void realtimeBar(int reqId, long time, double open, double high, double low, double close,
            long volume, double wap, int count)
        {
            Console.WriteLine("RealTimeBars. " + reqId + " - Time: " + UnixTimeStampToDateTime(Convert.ToDouble(time)) +
                              ", Open: " + open + ", High: " + high + ", Low: " + low + ", Close: " + close +
                              ", Volume: " + volume + ", Count: " + count + ", WAP: " + wap + "\n");
        }

        public virtual void historicalData(int reqId, string date, double open, double high, double low, double close,
            int volume, int count, double wap, bool hasGaps)
        {
            //Console.WriteLine("HistoricalData. " + reqId + " - Date: " + date + ", Open: " + open + ", High: " + high +
            //                  ", Low: " + low + ", Close: " + close + ", Volume: " + volume + ", Count: " +
            //                  count +
            //                  ", WAP: " + wap + ", HasGaps: " + hasGaps + "\n");

            switch (reqId)
            {
                //case 40002:
                //    Program.Eur.Add(new BarData2(open, high, low, close*125000*1d, volume, date, "EUR"));
                //    break;
                //case 40003:
                //    Program.Gbp.Add(new BarData2(open, high, low, close*62500*1d, volume, date, "GBP"));
                //    break;
                //case 40004:
                //    Program.Jpy.Add(new BarData2(open, high, low, close*12500000*1d, volume, date, "JPY"));
                //    break;
                //case 40006:
                //    Program.Cad.Add(new BarData2(open, high, low, close*100000*1d, volume, date, "CAD"));
                //    break;
                //case 40005:
                //    Program.Aud.Add(new BarData2(open, high, low, close*100000*1d, volume, date, "AUD"));
                //break;
                //case 40011:
                //    Program.Eurminute.Add(new MinuteBar(open, high, low, close*125000*1d, volume, DateTime.Now, "EUR"));
                //    break;
                //case 40022:
                //    Program.Gbpminute.Add(new MinuteBar(open, high, low, close*62500*1d, volume, DateTime.Now, "GBP"));
                //    break;
                //case 40033:
                //    Program.Jpyminute.Add(new MinuteBar(open, high, low, close*12500000*1d, volume, DateTime.Now, "JPY"));
                //    break;
                //case 40044:
                //    Program.Cadminute.Add(new MinuteBar(open, high, low, close*100000*1d, volume, DateTime.Now, "CAD"));
                //    break;
                //case 40055:
                //    Program.Audminute.Add(new MinuteBar(open, high, low, close*100000*1d, volume, DateTime.Now, "AUD"));
                //    break;
                //case 400111:
                //    Program.Eurtick.Add(new Tick(close*125000*1d, DateTime.Now, "EUR"));
                //    break;
                //case 400222:
                //    Program.Gbptick.Add(new Tick(close*62500*1d, DateTime.Now, "GBP"));
                //    break;
                //case 400333:
                //    Program.Jpytick.Add(new Tick(close*12500000*1d, DateTime.Now, "JPY"));
                //    break;
                //case 400444:
                //    Program.Cadtick.Add(new Tick(close*100000*1d, DateTime.Now, "CAD"));
                //    break;
                //case 400555:
                //    Program.Audtick.Add(new Tick(close*100000*1d, DateTime.Now, "AUD"));
                //break;
            }
            //Data.BarListData2.Add(new BarData2(open, high, low, close, volume, date));
            ////var tf = new TaskFactory();
            //_tf.StartNew(() =>
            //    NetMqMessanger.HandleNewBar(open, high, low, close, date));
        }

        public virtual void connectionClosed()
        {
            Console.WriteLine("Connection closed.\n");
        }

        public virtual void currentTime(long time)
        {
            Console.WriteLine("Current Time: " + UnixTimeStampToDateTime(Convert.ToDouble(time)) + "\n");
        }

        public virtual void tickPrice(int tickerId, int field, double price, int canAutoExecute)
        {
            if (field == 4)
            {
                var close = price;

                switch (tickerId)
                {
                }

                //Data.TickList.Add(new Tick(price, DateTime.Now.ToString("mm:ss.fff")));
                //Data.LastPrice = price;
                //////var tick = new Tick(price, DateTime.Now.ToString("mm:ss.fff"));

                Console.WriteLine("Tick Price. Ticker Id:" + tickerId + ", Field: " + field + ", Price: " + price +
                                  ", CanAutoExecute: " + canAutoExecute);
                //Console.WriteLine("***************************");
                //Console.WriteLine(DateTime.Now.ToString("mm:ss.fff"));
                //_tf.StartNew(() => NetMqMessanger.HandleTick(price));

                //Strategy.Director();
            }
        }

        public virtual void tickSize(int tickerId, int field, int size)
        {
            //Console.WriteLine("Tick Size. Ticker Id:" + tickerId + ", Field: " + field + ", Size: " + size+"\n");
        }

        public virtual void tickString(int tickerId, int tickType, string value)
        {
            /*Console.WriteLine("Tick string. Ticker Id:" + tickerId + ", Type: " + tickType + ", Value: " + UnixTimeStampToDateTime(Convert.ToDouble(value)));
            Console.Write(DateTime.Now.ToString("mm:ss.fff"));
            Console.WriteLine("***************************"+"\n");*/
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
            Console.WriteLine("TickSnapshotEnd: " + tickerId + "\n");
        }

        public virtual void nextValidId(int orderId)
        {
            Console.WriteLine("Next Valid Id: " + orderId + "\n");
            NextOrderId = orderId;
        }

        public virtual void deltaNeutralValidation(int reqId, UnderComp underComp)
        {
        }
        public virtual void updateAccountTime(string timestamp)
        {
            //Console.WriteLine("UpdateAccountTime. Time: " + timestamp + "\n");
        }

        public virtual void accountDownloadEnd(string account)
        {
            Console.WriteLine("Account download finished: " + account + "\n");
        }
        public virtual void managedAccounts(string accountsList)
        {
            Console.WriteLine("Account list: " + accountsList + "\n");
        }

        public virtual void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta,
            double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
        }

        public virtual void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            Console.WriteLine("Acct Summary. ReqId: " + reqId + ", Acct: " + account + ", Tag: " + tag + ", Value: " +
                              value + ", Currency: " + currency + "\n");
        }

        public virtual void accountSummaryEnd(int reqId)
        {
            Console.WriteLine("AccountSummaryEnd. Req Id: " + reqId + "\n");
        }

        public virtual void updateAccountValue(string key, string value, string currency, string accountName)
        {
            Console.WriteLine("UpdateAccountValue. Key: " + key + ", Value: " + value + ", Currency: " + currency +
                              ", AccountName: " + accountName + "\n");
            NumberFormatInfo provider = new NumberFormatInfo
            {
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = ".",
                NumberGroupSizes = new int[] {3}
            };

            if ((key == "NetLiquidation" || key == "CashBalance" || key == "DayTradesRemaining" ||
                 key == "EquityWithLoanValue" || key == "InitMarginReq" || key == "MaintMarginReq"
                 || key == "UnrealizedPnL") && currency == "USD")
            {
                if (key == "NetLiquidation" || key == "UnrealizedPnL")
                {
                    Console.WriteLine("UpdateAccountValue. Key: " + key + ", Value: " + value + ", Currency: " +
                                      currency +
                                      ", AccountName: " + accountName + "\n");
                    if (key == "NetLiquidation") Equity = Convert.ToDouble(value, provider);
                }

               // tf.StartNew((() => NetMqMessanger.HandleAccountUpdate(accountName, key, value)));
            }
        }

        public virtual void UpdatePortfolio(Contract contract, int position, double marketPrice, double marketValue,
            double averageCost, double unrealisedPnl, double realisedPnl, string accountName)
        {
            if (contract.SecType == "FUT")
            {
                Console.WriteLine("UpdatePortfolio. " + contract.Symbol + ", " + contract.SecType + " @ " +
                                  contract.Exchange
                                  + ": Quantity: " + position + ", MarketPrice: " + marketPrice + ", MarketValue: " +
                                  marketValue + ", AveragePrice: " + averageCost
                                  + ", UnrealisedPNL: " + unrealisedPnl + ", RealisedPNL: " + realisedPnl +
                                  ", AccountName: " + accountName + "\n");
                //tf.StartNew(
                //    (() =>
                //        NetMqMessanger.HandlePortfolioUpdate(accountName, contract.Symbol, position,
                //        marketPrice, marketValue, averageCost, realisedPnl, unrealisedPnl)));
            }
        }

       

        public virtual void OrderStatus(int orderId, string status, int filled, int remaining, double avgFillPrice,
            int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            Console.WriteLine("OrderStatus. Id: " + orderId + ", Status: " + status + ", Filled" + filled +
                              ", Remaining: " + remaining
                              + ", AverageFillPrice: " + avgFillPrice + ", PermanentId: " + permId + ", ParentId: " + parentId +
                              ", LastFillPrice: " + lastFillPrice + ", ClientId: " + clientId + ", WhyHeld: " + whyHeld +
                              "\n");
            tf.StartNew(() => netMqMessanger.HandleMessages(ObjectContructorHelper.GetOrderStatusMessage(orderId, status, filled, remaining,
                        avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld), GeneralRequestMessageType.OrderStatusPush));
        }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            Console.WriteLine("OpenOrder. ID: " + orderId + ", " + contract.Symbol + ", " + contract.SecType + " @ " +
                              contract.Exchange + ": " + order.Action + ", " + order.OrderType + " " +
                              order.TotalQuantity + ", " + orderState.Status + "\n");
            tf.StartNew(() => netMqMessanger.HandleMessages(ObjectContructorHelper.GetOpenOrder(contract, order, orderState), GeneralRequestMessageType.OpenOrderPush));
        }

        public virtual void execDetails(int reqId, Contract contract, Execution execution)
        {
            Console.WriteLine("ExecDetails. " + reqId + " - " + contract.Symbol + ", " + contract.SecType + ", " +
                              contract.Currency + " - " + execution.ExecId + ", " + execution.OrderId + ", " +
                              execution.Shares + "\n");
            tf.StartNew(() => netMqMessanger.HandleMessages(ObjectContructorHelper.GetExecutionMessage(reqId, contract, execution), GeneralRequestMessageType.ExecutionPush));
        }

        public virtual void commissionReport(CommissionReport commissionReport)
        {
            Console.WriteLine("CommissionReport. " + commissionReport.ExecId + " - " + commissionReport.Commission + " " +
                              commissionReport.Currency + " RPNL " + commissionReport.RealizedPNL + "\n");

            tf.StartNew(() => netMqMessanger.HandleMessages(commissionReport.Map<CommissionReport, CommissionMessage>(), GeneralRequestMessageType.CommissionPush));

        }


        public virtual void openOrderEnd()
        {
            Console.WriteLine("OpenOrderEnd");
        }

        public virtual void contractDetails(int reqId, ContractDetails contractDetails)
        {
            Console.WriteLine("ContractDetails. ReqId: " + reqId + " - " + contractDetails.Summary.Symbol + ", " +
                              contractDetails.Summary.SecType + ", ConId: " + contractDetails.Summary.ConId + " @ " +
                              contractDetails.Summary.Exchange + "\n");
        }

        public virtual void contractDetailsEnd(int reqId)
        {
            Console.WriteLine("ContractDetailsEnd. " + reqId + "\n");
        }

        public virtual void execDetailsEnd(int reqId)
        {
            Console.WriteLine("ExecDetailsEnd. " + reqId + "\n");
        }

        public virtual void fundamentalData(int reqId, string data)
        {
            Console.WriteLine("FundamentalData. " + reqId + "" + data + "\n");
        }

        public virtual void marketDataType(int reqId, int marketDataType)
        {
            Console.WriteLine("MarketDataType. " + reqId + ", Type: " + marketDataType + "\n");
        }

        public virtual void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            Console.WriteLine("UpdateMarketDepth. " + tickerId + " - Quantity: " + position + ", Operation: " +
                              operation + ", Side: " + side + ", Price: " + price + ", Size" + size + "\n");
        }

        public virtual void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side,
            double price, int size)
        {
            Console.WriteLine("UpdateMarketDepthL2. " + tickerId + " - Quantity: " + position + ", Operation: " +
                              operation + ", Side: " + side + ", Price: " + price + ", Size" + size + "\n");
        }

        public virtual void updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
        {
            Console.WriteLine("News Bulletins. " + msgId + " - Type: " + msgType + ", Message: " + message +
                              ", Exchange of Origin: " + origExchange + "\n");
        }

        public virtual void Position(string account, Contract contract, int pos, double avgCost)
        {
            Console.WriteLine("Quantity. " + account + " - Symbol: " + contract.Symbol + ", SecType: " +
                              contract.SecType + ", Currency: " + contract.Currency + ", Quantity: " + pos +
                              ", Avg cost: " + avgCost + "\n");
        }

        public virtual void positionEnd()
        {
            Console.WriteLine("PositionEnd \n");
        }

        public virtual void scannerParameters(string xml)
        {
            Console.WriteLine("ScannerParameters. " + xml + "\n");
        }

        public virtual void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance,
            string benchmark, string projection, string legsStr)
        {
            Console.WriteLine("ScannerData. " + reqId + " - Rank: " + rank + ", Symbol: " +
                              contractDetails.Summary.Symbol + ", SecType: " + contractDetails.Summary.SecType +
                              ", Currency: " + contractDetails.Summary.Currency
                              + ", Distance: " + distance + ", Benchmark: " + benchmark + ", Projection: " + projection +
                              ", Legs String: " + legsStr + "\n");
        }

        public virtual void scannerDataEnd(int reqId)
        {
            Console.WriteLine("ScannerDataEnd. " + reqId + "\n");
        }

        public virtual void receiveFA(int faDataType, string faXmlData)
        {
            Console.WriteLine("Receing FA: " + faDataType + " - " + faXmlData + "\n");
        }

        public virtual void bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            Console.WriteLine("Bond. Symbol " + contractDetails.Summary.Symbol + ", " + contractDetails.Summary);
        }

        public virtual void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            Console.WriteLine("Historical data end - " + reqId + " from " + startDate + " to " + endDate + " TIME:" +
                              DateTime.UtcNow.ToString("ss.ffffff"));
        }

        public virtual void verifyMessageAPI(string apiData)
        {
            Console.WriteLine("verifyMessageAPI: " + apiData);
        }

        public virtual void verifyCompleted(bool isSuccessful, string errorText)
        {
            Console.WriteLine("verifyCompleted. IsSuccessfule: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            Console.WriteLine("verifyAndAuthMessageAPI: " + apiData + " " + xyzChallenge);
        }

        public virtual void verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            Console.WriteLine("verifyAndAuthCompleted. IsSuccessful: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void displayGroupList(int reqId, string groups)
        {
            Console.WriteLine("DisplayGroupList. Request: " + reqId + ", Groups" + groups);
        }

        public virtual void displayGroupUpdated(int reqId, string contractInfo)
        {
            Console.WriteLine("displayGroupUpdated. Request: " + reqId + ", ContractInfo: " + contractInfo);
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
    }
    
}