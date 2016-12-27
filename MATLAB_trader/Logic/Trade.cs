using System;
using System.Threading;
using IBApi;

namespace MATLAB_trader.Logic
{
    public class Trade
    {
        private static readonly Random R = new Random();
        private readonly string _account;

        /// <summary>
        ///     Trades the specified contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="i">The i.</param>
        /// <param name="wrapper"></param>
        public static void PlaceTrade(Contract contract, int i, IbClient wrapper)
        {
            if (i >= 1)
            {
                MakeMktTrade(contract, "BUY", "MKT", Convert.ToInt32(i), wrapper);
                Thread.Sleep(10000);
                MakeMktTrade(contract, "BUY", "MKT", Convert.ToInt32(i), wrapper);
                Thread.Sleep(10000);
            }
            else if (i <= -1)
            {
                MakeMktTrade(contract, "SELL", "MKT", Convert.ToInt32(i), wrapper);
                Thread.Sleep(10000);
                MakeMktTrade(contract, "BUY", "MKT", Convert.ToInt32(i), wrapper);
                Thread.Sleep(10000);
            }
        }

        public static void MakeMktTrade(Contract contract, string direction, string type, int quantity, IbClient wrapper)
        {
            var order = new Order
            {
                Action = direction,
                OrderType = type,
                Account = wrapper.AccountNumber,
                TotalQuantity = Math.Abs(quantity),
                OrderId = wrapper.NextOrderId++,
                Tif = "GTC"
            };

            wrapper.ClientSocket.placeOrder(order.OrderId, contract, order);
            //Thread.Sleep(5000);
        }

        public static void MakeLmtTrade(Contract contract, string direction, string type, int quantity, double price,
            IbClient wrapper)
        {
            var order = new Order
            {
                Action = direction,
                OrderType = type,
                LmtPrice = price,
                Account = wrapper.AccountNumber,
                TotalQuantity = quantity,
                OrderId = wrapper.NextOrderId++,
                Tif = "GTC"
            };

            wrapper.ClientSocket.placeOrder(order.OrderId, contract, order);
        }
    }
}