using System;
using System.Collections.Generic;
using System.Threading;
using IBApi;

namespace MATLAB_trader.Logic
{
    public class Trade
    {
        private static readonly Contract futureComboContract = new Contract
        {
            Symbol = "GC",
            SecType = "FUT",
            Exchange = "NYMEX",
            Currency = "USD",
            LastTradeDateOrContractMonth = "201705"
        };



        //public static void PlaceTrade(Contract contract, int i, IbClient wrapper)
        //{
        //    if (i >= 1)
        //    {
        //        MakeLmtTrade(wrapper);
        //        Thread.Sleep(10000);
        //        MakeMktTrade(contract, "BUY", "MKT", Convert.ToInt32(i), wrapper);
        //        Thread.Sleep(10000);
        //        MakeMktTrade(contract, "SELL", "MKT", Convert.ToInt32(i), wrapper);
        //        Thread.Sleep(10000);
        //        wrapper.ClientSocket.reqGlobalCancel();

        //    }
        //    else if (i <= -1)
        //    {
        //        MakeLmtTrade(wrapper);
        //        Thread.Sleep(10000);
        //        MakeMktTrade(contract, "SELL", "MKT", Convert.ToInt32(i), wrapper);
        //        Thread.Sleep(10000);
        //        MakeMktTrade(contract, "BUY", "MKT", Convert.ToInt32(i), wrapper);
        //        Thread.Sleep(10000);
        //        wrapper.ClientSocket.reqGlobalCancel();

        //    }
        //}

        /// <summary>
        ///     Trades the specified contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="direction"></param>
        /// <param name="wrapper"></param>
        /// <param name="type"></param>
        /// <param name="quantity"></param>
        public static void MakeMktTrade(string direction , IbClient wrapper, Contract contract = null, string type = "MKT", double quantity = 1)
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

            wrapper.ClientSocket.placeOrder(order.OrderId, FutureComboContract(), order);
            //order.Action = direction == "SELL" ? "BUY" : "SELL";
            //order.OrderId = wrapper.NextOrderId++;
            //wrapper.ClientSocket.placeOrder(order.OrderId, FutureComboContract(), order);
            //Thread.Sleep(5000);
        }
        public static Contract FutureComboContract() => futureComboContract;

        public static void MakeLmtTrade( 
            IbClient wrapper, double price=3000, Contract contract=null, int quantity=1, string direction="SELL", string type= "LMT")
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

            wrapper.ClientSocket.placeOrder(order.OrderId, (contract ?? futureComboContract), order);
        }
    }
}