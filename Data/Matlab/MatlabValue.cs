namespace MATLAB_trader.Data.Matlab
{
    public abstract class MatlabValue
    {
       
        public string Symbol { get; set; }
        public double Value { get; set; }
        public string Account { get; set; }
        public decimal Equity { get; set; }
        public MatlabType ValueType { get; set; }
        public string Name { get; set; }
    }
}