using System;
using System.Collections.ObjectModel;
using IBApi;

namespace MATLAB_trader.Data.DataType
{
    public class ExecutionMessage
    {
        public static ObservableCollection<ExecutionMessage> ExecutionMessageList =
            new ObservableCollection<ExecutionMessage>();

        public ExecutionMessage(int reqId, Contract contract, Execution execution)
        {
            ReqId = reqId;
            ExecutionId = execution.ExecId;
            PermId = execution.PermId;
            ContractSymbol = contract.Symbol;
            ContractSecType = contract.SecType;
            AccountNumber = execution.AcctNumber;
            Qty = execution.CumQty;
            Side = execution.Side;
            OrderId = execution.OrderId;
            Price = execution.Price;
            Time = DateTime.Now;
        }

        public int ReqId { get; set; }
        public string ExecutionId { get; set; }
        public string AccountNumber { get; set; }
        public int PermId { get; set; }
        public int OrderId { get; set; }
        public string ContractSecType { get; set; }
        public string ContractSymbol { get; set; }
        public int Qty { get; set; }
        public string Side { get; set; }
        public double Price { get; set; }
        public DateTime Time { get; set; }
    }

    public class ExecutionMessageExtended : ExecutionMessage
    {
        public ExecutionMessageExtended(int reqId, Contract contract, Execution execution)
            : base(reqId, contract, execution)
        {
        }
    }
}