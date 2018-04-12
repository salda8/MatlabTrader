using System;
using IBApi;

namespace StrategyTrader.Logic
{
    public class Trade
    {
        private static readonly Contract futureComboContract = new Contract
        {
            Symbol = "GC",
            SecType = "FUT",
            Exchange = "NYMEX",
            Currency = "USD",
            LastTradeDateOrContractMonth = "201708"
        };

        public static void PlaceMarketOrder(Contract contract, double i, IbClient wrapper)
        {
            if (i >= 1)
            {
                MakeMktTrade("BUY", wrapper, contract, "MKT", i);


            }
            else if (i <= -1)
            {

                MakeMktTrade("SELL", wrapper, contract, "MKT", i); 
               

            }
        }
       
        public static void PlaceOrder(IbClient wrapper, Order order, Contract contract)
        {
            order.OrderId = wrapper.NextOrderId;
            wrapper.ClientSocket.placeOrder(order.OrderId, (contract ?? futureComboContract), order);
            //it does not really matter how fast this method execute since 
            //this is only used when for limit/stop when rolling over contracts
            //so lets add random delay to ensure no problems with order ids
            AddRandomDelay();
        }

        private static void AddRandomDelay()
        {
            Random random = new Random();
            var mseconds = random.Next(3, 10) * 10;
            System.Threading.Thread.Sleep(mseconds);
        }

        /// <summary>
        ///     Trades the specified contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="direction"></param>
        /// <param name="wrapper"></param>
        /// <param name="type"></param>
        /// <param name="quantity"></param>
        private static void MakeMktTrade(string direction, IbClient wrapper, Contract contract = null, string type = "MKT", double quantity = 1)
        {
            var order = new Order
            {
                Action = direction,
                OrderType = type,
                Account = wrapper.AccountNumber,
                TotalQuantity = Math.Abs(quantity),
                OrderId = wrapper.NextOrderId,
                Tif = "GTC"
            };

            wrapper.ClientSocket.placeOrder(order.OrderId, (contract ?? futureComboContract), order);
            
        }

        public static Contract FutureComboContract() => futureComboContract;

        public static void MakeLmtTrade(
            IbClient wrapper, double price = 3000, Contract contract = null, int quantity = 1, string direction = "SELL", string type = "LMT")
        {
            var order = new Order
            {
                Action = direction,
                OrderType = type,
                LmtPrice = price,
                Account = wrapper.AccountNumber,
                TotalQuantity = quantity,
                OrderId = wrapper.NextOrderId,
                Tif = "GTC"
            };

            wrapper.ClientSocket.placeOrder(order.OrderId, (contract ?? futureComboContract), order);
        }

        

        public static void MakeLmtTrade1(
            IbClient wrapper, double price = 3000, Contract contract = null, int quantity = 1, string direction = "SELL", string type = "LMT")
        {
            var order = new Order
            {
                Action = direction,
                OrderType = type,
                LmtPrice = price,
                AdjustedStopPrice =2500,
                Account = wrapper.AccountNumber,
                TotalQuantity = quantity,
                OrderId = wrapper.NextOrderId,
                Tif = "GTC"
            };

            wrapper.ClientSocket.placeOrder(order.OrderId, (contract ?? futureComboContract), order);
        }
    }
}