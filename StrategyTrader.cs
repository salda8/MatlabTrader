using System;
using System.Threading;
using MATLAB_trader.Data.DataType;
using MATLAB_trader.Logic;

namespace MATLAB_trader
{
    public class StrategyTrader
    {
        private Matlab matlab;
        public StrategyTrader()
        {
            InitializeMatlab();
        }

        private void InitializeMatlab()
        {
            var activationContext = Type.GetTypeFromProgID("matlab.application.single");
            matlab = new Matlab((MLApp.MLApp)Activator.CreateInstance(activationContext));
            
           
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

                    matlab.Execute(DateTime.Now);
                }
                Thread.Sleep(10000);
            }
        }
    }

}