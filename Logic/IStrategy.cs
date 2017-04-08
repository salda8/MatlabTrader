using System;

namespace MATLAB_trader.Logic
{
    internal interface IStrategy
    {
        void Execute();
        void StartTrading();
    }
}