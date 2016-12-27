using IBApi;

namespace MATLAB_trader.Data.DataType
{
    public class CommissionMessage
    {
        public CommissionMessage(CommissionReport commissionReport)
        {
            Commission = commissionReport.Commission;
            RealizedPnL = commissionReport.RealizedPNL;
            ExecutionId = commissionReport.ExecId;
        }

        public string ExecutionId { get; set; }
        public double Commission { get; set; }
        public double RealizedPnL { get; set; }
    }
}