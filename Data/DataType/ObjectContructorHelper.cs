using System;
using System.Collections.ObjectModel;
using IBApi;
using QDMS;

namespace MATLAB_trader.Data.DataType
{
    public static class ObjectContructorHelper
    {
        public static QDMS.ExecutionMessage GetExecutionMessage(int reqId, Contract contract,
                                                         Execution execution)
        {
            return new QDMS.ExecutionMessage()
            {
                ReqId = reqId,
                ExecutionId = execution.ExecId,
                PermId = execution.PermId,
                ContractSymbol = contract.Symbol,
                ContractSecType = contract.SecType,
                AccountNumber = execution.AcctNumber,
                Qty = execution.CumQty,
                Side = execution.Side,
                OrderId = execution.OrderId,
                Price = execution.Price,
                Time = DateTime.Now
            };
        }

        public static QDMS.OpenOrder GetOpenOrder(Contract contract, Order order, OrderState orderState)
        {
            return new OpenOrder()
                   {
                
                       PermId = order.PermId,
                       AccountID = Program.AccountID,
                        InstrumentID = 1,//todo
                        Status = orderState.Status,
                     
                       LimitPrice = (decimal) order.LmtPrice,
                       Position = (decimal) order.TotalQuantity,
                       Type = order.OrderType
                       
                   };

          
        }
    }
}