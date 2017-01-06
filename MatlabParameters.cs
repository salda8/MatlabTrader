namespace MATLAB_trader.Logic
{
    internal partial class Matlab
    {

        public class MatlabParameters : MatlabValue
        {
            private static readonly MatlabType _matlabValueType = MatlabType.Parameter;
            public new MatlabType ValueType { get; set; } = _matlabValueType;
        }

       
    }
}