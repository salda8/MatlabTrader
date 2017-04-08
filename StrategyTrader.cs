using System;
using System.Threading;
using MATLAB_trader.Data.DataType;
using MATLAB_trader.Logic;

namespace MATLAB_trader
{
    public class StrategyTrader
    {
        private IStrategy strategy;
        public StrategyTrader(IbClient wrapper, bool useMatlab = false)
        {

            if (useMatlab)
            {
                InitMatlabStrategy();
            }
            else
            {
                InitNetStrategy(wrapper);
            }
        }

        private void InitNetStrategy(IbClient wrapper)
        {
            strategy = new SimplestNetStrategy(wrapper);
        }

        private void InitMatlabStrategy()
        {
            var activationContext = Type.GetTypeFromProgID("matlab.application.single");
            strategy = new MatlabStrategy((MLApp.MLApp)Activator.CreateInstance(activationContext));
            
           
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

                    strategy.Execute();
                    Thread.Sleep(10000);
                }
                Thread.Sleep(10000);
            }
        }
    }

}