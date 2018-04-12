using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Requests;
using IBApi;
using StrategyTrader.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RequestsClient = RequestsClient.RequestsClient;

namespace StrategyTrader.Logic
{
    internal class SimplestNetStrategy : IStrategy
    {
        private readonly IbClient wrapper;
        private DataRequestClient.DataRequestClient client;
        private List<OHLCBar> data;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private HistoricalDataRequest historicalDataRequest;

        private int dataCount;

        private TradingCalendar calendar;
        private Instrument instrument;
        private Contract contract;
        private Contract oldContract;
        private global::RequestsClient.RequestsClient requestClient;

        public SimplestNetStrategy(IbClient wrapper)
        {
            this.wrapper = wrapper;

            InitialiseClients();
            
            GetInstrumentAndContract();
            
        }

        private void InitialiseClients()
        {
            client = new DataRequestClient.DataRequestClient(Settings.Default.AccountNumber, Settings.Default.host,
                Settings.Default.RealTimeDataServerRequestPort, Settings.Default.RealTimeDataServerPublishPort,
                Settings.Default.HistoricalServerPort);
            client.Connect();
            client.HistoricalDataReceived += HistoricalDataReceived;
            requestClient = new global::RequestsClient.RequestsClient(Settings.Default.AccountID,
                Settings.Default.InstrumetnUpdateRequestSocketPort);
            wrapper.ClientSocket.reqAccountUpdates(true, Settings.Default.AccountNumber);
        }

        private void HistoricalDataReceived(object sender, HistoricalDataEventArgs e)
        {
            dataCount = e.Data.Count;
            data = new List<OHLCBar>(dataCount);
            data.AddRange(e.Data);
        }

        public void Execute()
        {
            //if (data?.Count > 0)
            //{
            //var ma1 = SimpleMovingAverageFunction(10);
            //var ma2 = SimpleMovingAverageFunction(15);
            //if (ma1[ma1.Length - 1] > ma2[ma2.Length - 1])
            //{
            //    Trade.MakeMktTrade("BUY", wrapper);
            //}
            //else if (ma1[ma1.Length - 1] < ma2[ma2.Length - 1])
            //{
            //    Trade.MakeMktTrade("SELL", wrapper);
            //}

            Task.Run(() => Trade.PlaceMarketOrder(contract, Common.Utils.Tools.IsOdd(DateTime.Now.Minute) ? 1 : -1, wrapper));
            Task.Run(() => Trade.MakeLmtTrade1(wrapper, 3000, contract));

            Thread.Sleep(10000);
            RolloverPositionAndContract();
            //wrapper.ClientSocket.reqPositions();
            //}
            //else
            //{
            //    RequestNewData();
            //    Thread.Sleep(1000);

            //}
        }

        private void RequestNewData() => client.RequestHistoricalData(historicalDataRequest);

        private decimal[] SimpleMovingAverageFunction(int period)
        {
            decimal[] buffer = new decimal[period];
            decimal[] output = new decimal[dataCount];
            var currentIndex = 0;
            for (int i = 0; i < dataCount; i++)
            {
                buffer[currentIndex] = data[i].Close / period;
                decimal ma = 0;
                for (int j = 0; j < period; j++)
                {
                    ma += buffer[j];
                }
                output[i] = ma;
                currentIndex = (currentIndex + 1) % period;
            }
            return output;
        }

        public void StartTrading()
        {
            while (calendar.IsTradingDay())
            {
                if (calendar.IsRolloverDay)
                {

                    RolloverPositionAndContract();
                }

                while (calendar.IsTradingHour())
                {
                    while (HighResolutionDateTime.UtcNow.Second != 0)
                    {

                        Thread.Sleep(10000);
                        break;
                    }

                    Execute();
                    //Thread.Sleep(10000);
                }
                Thread.Sleep(10000);
            }
        }

        private void RolloverPositionAndContract()
        {
            logger.Info(() => "Started roll over of contracts.");
            GetInstrumentAndContract();
            CloseAndReopenPositions();
        }

        private void CloseAndReopenPositions()
        {
            wrapper.ShouldCollectOpenOrders = true;
            wrapper.OpenOrderEndEnded = false;
            wrapper.ClientSocket.reqOpenOrders();
            wrapper.ClientSocket.reqPositions();

            while (!wrapper.OpenOrderEndEnded)
            {
                Thread.Sleep(100);
            }
            

            var wrapperLiveOrderQuantity = wrapper.LiveOrderQuantity;
            List<Order> wrapperOpenOrders = new List<Order>(wrapper.OpenOrders);
            wrapper.ShouldCollectOpenOrders = false;
            wrapper.OpenOrders.Clear();
            wrapper.ClientSocket.reqGlobalCancel();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (wrapperLiveOrderQuantity != 0)
            {
                Trade.PlaceMarketOrder(oldContract, -wrapperLiveOrderQuantity, wrapper);
                Trade.PlaceMarketOrder(contract, wrapperLiveOrderQuantity, wrapper);
            }

            
            Thread.Sleep(2000);//wait for open order to get canceled..
            foreach (var order in wrapperOpenOrders)
            {
                Trade.PlaceOrder(wrapper, order, contract);
            }

            logger.Info(() => $"New FUTURES contract: {instrument.ToString()}");
            
            logger.Info(
                () =>
                    $"Roll over of contracts has finished. MARKET positions reopened:{wrapperLiveOrderQuantity}. OPEN ORDER positions reopened:{wrapperOpenOrders.Count}.");
        }

        private void GetInstrumentAndContract()
        {
             
            instrument = requestClient.RequestActiveInstrumentContract(Settings.Default.StrategyID);
            oldContract = contract;
            contract = InstrumentToContract(instrument);
            historicalDataRequest = new HistoricalDataRequest()
            {
                Instrument = instrument,
                Frequency = BarSize.OneHour,
                EndingDate = DateTime.Now,
                StartingDate = DateTime.Now.AddDays(-3),
                DataLocation = DataLocation.ExternalOnly,
                RTHOnly = false,
                SaveToLocalStorage = false
            };

            Settings.Default.InstrumentId = instrument.ID;
            Settings.Default.Save();

            wrapper.ClientSocket.reqAccountUpdates(true, Settings.Default.AccountNumber);
            
            calendar = new TradingCalendar(instrument.ExpirationRule, instrument.Expiration, instrument.Sessions, instrument.Exchange.Timezone);
        }

        public static Contract InstrumentToContract(Instrument instrument) => new Contract()
        {
            Symbol = instrument.UnderlyingSymbol,
            SecType = Common.Utils.GetDescriptionHelper.GetDescription(instrument.Type, string.Empty),
            LastTradeDateOrContractMonth = instrument.Expiration.ToString("yyyyMM", CultureInfo.InvariantCulture),
            Currency = instrument.Currency,
            PrimaryExch = instrument.Exchange.Name,
            Exchange = instrument.Exchange.Name,
            IncludeExpired = false
        };
    }
}