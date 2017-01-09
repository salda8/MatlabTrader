using System;
using IBApi;
using QDMS;

namespace MATLAB_trader.Data.DataType
{
    public static class ObjectContructorHelper
    {
        public static ExecutionMessage GetExecutionMessage(int reqId, Contract contract,
                                                         Execution execution)
        {
            return new ExecutionMessage
                   {
                RequestId = reqId,
                ExecutionId = execution.ExecId,
                PermanentId = execution.PermId,
                InstrumentID = GetInstrumentId(contract.Symbol),
                AccountID = Program.AccountID,
                Quantity = execution.CumQty,
                Side = execution.Side,
                OrderId = execution.OrderId,
                Price = new decimal(execution.Price),
                Time = DateTime.Now
            };
        }

        public static OpenOrder GetOpenOrder(Contract contract, Order order, OrderState orderState)
        {
            return new OpenOrder
                   {
                PermanentId = order.PermId,
                       AccountID = Program.AccountID,
                        InstrumentID = GetInstrumentId(contract.LocalSymbol),
                        Status = orderState.Status,
                     LimitPrice = (decimal) order.LmtPrice,
                       Quantity = (decimal) order.TotalQuantity,
                       Type = order.OrderType
                       
                   };

          
        }

        public static OrderStatusMessage GetOrderStatusMessage(int orderId, string status, int filled, int remaining, double averageFillPrice,
            int permanentId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            return new OrderStatusMessage
            { 
                       OrderId = orderId,
                       Status = status,
                       Filled = filled,
                       Remaining = remaining,
                       AverageFillPrice = new decimal(averageFillPrice),
                       PermanentId = permanentId,
                       ParentId = parentId,
                       LastFillPrice = new decimal(lastFillPrice),
                       ClientId = clientId,
                       WhyHeld = whyHeld
                   };
        }

        private static int GetInstrumentId(string srcDescription)
        {
            return 1;
        }
    }
}