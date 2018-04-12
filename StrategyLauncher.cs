using StrategyTrader.Logic;

namespace StrategyTrader
{
    public class StrategyLauncher
    {
        private IStrategy strategy;

        public StrategyLauncher(IbClient wrapper, bool useMatlab = false)
        {
            if (useMatlab)
            {
               //InitMatlabStrategy();
            }
            else
            {
                InitNetStrategy(wrapper);
            }
        }

        private void InitNetStrategy(IbClient wrapper)
        {
            strategy = new SimplestNetStrategy(wrapper);
            strategy.StartTrading();
        }

        //private void InitMatlabStrategy()
        //{
        //    var activationContext = Type.GetTypeFromProgID("matlab.application.single");
        //    strategy = new MatlabStrategy((MLApp.MLApp)Activator.CreateInstance(activationContext));
        //    Task.Run(()=>strategy.StartTrading());

        //}
    }
}