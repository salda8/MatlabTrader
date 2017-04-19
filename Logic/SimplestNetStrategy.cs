using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Requests;
using IBApi;
using StrategyTrader.Properties;

namespace StrategyTrader.Logic
{
    internal class SimplestNetStrategy : IStrategy
    {
        private readonly IbClient wrapper;
        private readonly DataRequestClient.DataRequestClient client;
        private List<OHLCBar> data;
        
        private HistoricalDataRequest historicalDataRequest;

        private int dataCount;
        
        private TradingCalendar calendar;
        private Instrument instrument;
        private Contract contract;

        public SimplestNetStrategy(IbClient wrapper)
        {
            this.wrapper = wrapper;

            client = new DataRequestClient.DataRequestClient(Settings.Default.AccountNumber, Settings.Default.host,
                Settings.Default.RealTimeDataServerRequestPort, Settings.Default.RealTimeDataServerPublishPort,
                Settings.Default.HistoricalServerPort);
            client.Connect();
            client.HistoricalDataReceived += HistoricalDataReceived;
            GetInstrumentAndContract();
           

        }

        private void HistoricalDataReceived(object sender, HistoricalDataEventArgs e)
        {
            dataCount = e.Data.Count;
            data = new List<OHLCBar>(dataCount);
            data.AddRange(e.Data);

        }

        public void Execute()
        {
            if (data?.Count > 0)
            {
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
                Trade.MakeMktTrade(Common.Utils.Tools.IsOdd(DateTime.Now.Minute) ? "BUY" : "SELL", wrapper, contract);
                Trade.MakeLmtTrade(wrapper, 3000);
                wrapper.ClientSocket.reqPositions();
            }
            else
            {
                RequestNewData();
                Thread.Sleep(1000);

            }
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
            while (TradingCalendar.IsTradingDay())
            {
                if (calendar.IsRolloverDay)
                {
                    RolloverPositionAndContract();
                }

                while (TradingCalendar.IsTradingHour())
                {
                    while (HighResolutionDateTime.UtcNow.Second != 0)
                    {
                        Thread.Sleep(1);

                    }

                    Execute();
                    Thread.Sleep(10000);
                }
                Thread.Sleep(10000);
            }
        }

        private void RolloverPositionAndContract()
        {
            GetInstrumentAndContract();
            //close position
            ClosePositions();
            //open under new contract
        }

        private void ClosePositions()
        {

        }

        private void GetInstrumentAndContract()
        {
            var requestClient = new RequestsClient.RequestsClient(Settings.Default.AccountID,
                Settings.Default.InstrumetnUpdateRequestSocketPort);
            instrument = requestClient.RequestActiveInstrumentContract(Settings.Default.StrategyID);
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

            Properties.Settings.Default.InstrumentId = instrument.ID;
            Properties.Settings.Default.Save();

            wrapper.ClientSocket.reqAccountUpdates(true, Settings.Default.AccountNumber);//todo it is possible to subscribe to that from server
            wrapper.ClientSocket.reqOpenOrders();
            wrapper.ClientSocket.reqPositions();

           calendar = new TradingCalendar(instrument.ExpirationRule, instrument.Expiration);

        }
    

        public static Contract InstrumentToContract(Instrument instrument) => new Contract()
        {
            Symbol = instrument.UnderlyingSymbol,
            SecType = Common.Utils.GetDescriptionHelper.GetDescription(instrument.Type, string.Empty),
            LastTradeDateOrContractMonth = instrument.Expiration.ToString("yyyyMM", CultureInfo.InvariantCulture),
            Currency = instrument.Currency,
            PrimaryExch = instrument.Exchange.Name,
            Exchange= instrument.Exchange.Name,
            IncludeExpired = false
        };
    }
}