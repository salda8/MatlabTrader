namespace StrategyTrader.Logic
{
    internal interface IStrategy
    {
        void Execute();
        void StartTrading();
    }
}