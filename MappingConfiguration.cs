using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressMapper;
using IBApi;
using MATLAB_trader.Data.DataType;
using CommissionMessage = QDMS.CommissionMessage;

namespace MATLAB_trader
{
    public class MappingConfiguration
    {
        public static void Register()
        {
            Mapper.Register<HistoricalTrade, QDMS.TradeHistory>()
                   .Value(dest => dest.AccountID, Program.AccountID)
                   .Member(dest => dest.Commission, src => src.Commission)
                   .Member(dest => dest.ExecId, src => src.ExecutionId)
                   .Member(x => x.InstrumentID, src => GetInstrumentId(src.Description))
                   .Member(dest => dest.Price, src => src.Price)
                   .Member(dest => dest.Position, src => src.Quantity)
                   .Member(dest => dest.RealizedPnL, src => src.RealizedPnL)
                   .Member(dest => dest.Side, src => src.Side)
                   .Member(dest => dest.ExecTime, src => src.ExecTime);

            Mapper.Register<CommissionReport, CommissionMessage>()
                  .Member(dest => dest.Commission, src => Convert.ToDecimal(src.Commission))
                  .Member(dest => dest.ExecutionId, src => src.ExecId)
                  .Member(dest => dest.RealizedPnL, src => src.RealizedPNL);

            
                
                
            Mapper.Compile();
        }
        private static int GetInstrumentId(string srcDescription)
        {
            return 1;
        }
    }
}
