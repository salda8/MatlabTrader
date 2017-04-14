using Common;
using Common.EntityModels;
using ExpressMapper;
using IBApi;

namespace StrategyTrader
{
    public class MappingConfiguration
    {
        public static void Register()
        {
            Mapper.Register<CommissionReport, CommissionMessage>()
                  .Member(dest => dest.Commission, src => new decimal(src.Commission))
                  .Member(dest => dest.ExecutionId, src => src.ExecId)
                  // ReSharper disable once CompareOfFloatsByEqualityOperator
                  .Member(dest => dest.RealizedPnL, src => (src.RealizedPNL == double.MaxValue) ? 1000000 : src.RealizedPNL);

            Mapper.Compile();
        }
    }
}