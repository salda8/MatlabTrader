using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Requests;
using StrategyTrader.Properties;

namespace StrategyTrader.Logic
{
    internal class SimplestNetStrategy : IStrategy
    {
        private readonly IbClient wrapper;
        private readonly DataRequestClient.DataRequestClient client;
        private List<OHLCBar> data;

        public static int InstrumentId => instrumentId==0? ( instrumentId= historicalDataRequest.Instrument.ID.Value): instrumentId;

        private static HistoricalDataRequest historicalDataRequest = new HistoricalDataRequest()
        {
            Instrument =
                new Instrument()
                {
                    ID=1,
                    Symbol = "GCM7",
                    UnderlyingSymbol = "GC",
                    Type = InstrumentType.Future,
                    Exchange = new Exchange { ID = 255, Name = "NYMEX", Timezone = "Eastern Standard Time" },
                    Datasource = new Datasource() { Name = "Interactive Brokers" },
                    Currency = "USD",
                    MinTick=Convert.ToDecimal(0.1),
                    Multiplier=100,
                    ValidExchanges="NYMEX",



                },
            Frequency = BarSize.OneHour,
            EndingDate = DateTime.Now,
            StartingDate = DateTime.Now.AddDays(-3),
            DataLocation = DataLocation.ExternalOnly,
            RTHOnly = false,
            SaveDataToStorage = false,
        };

        private int dataCount;
        private static int instrumentId;

        public SimplestNetStrategy(IbClient wrapper)
        {
            this.wrapper = wrapper;
            client = new DataRequestClient.DataRequestClient(Settings.Default.AccountNumber, Settings.Default.host,
                Settings.Default.RealTimeDataServerRequestPort, Settings.Default.RealTimeDataServerPublishPort,
                Settings.Default.HistoricalServerPort);
            client.Connect();
            client.HistoricalDataReceived += HistoricalDataReceived;
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
                Trade.MakeMktTrade(Common.Utils.Tools.IsOdd(DateTime.Now.Minute) ? "BUY" : "SELL", wrapper);
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
    }
}