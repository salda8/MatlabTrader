namespace MATLAB_trader.Logic
{
    internal partial class Matlab
    {


        public class MatlabValue
        {
            private static readonly MatlabType _matlabValueType = MatlabType.Parameter;
            public string Symbol { get; set; }
            public double Value { get; set; }
            public string Account { get; set; }
            public double Equity { get; set; }
            public MatlabType ValueType { get; set; } = _matlabValueType;
            public string Name { get; set; }
        }

    }
}