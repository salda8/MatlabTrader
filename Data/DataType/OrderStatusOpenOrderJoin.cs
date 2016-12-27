namespace MATLAB_trader.Data.DataType
{
    public class OrderStatusOpenOrderJoin
    {
        public OrderStatusOpenOrderJoin(int orderId, int permId, string account, string symbol, string sectype,
            string status, int filled, double price, double limitPrice, int totalQnt, int size, string orderState)
        {
            //OrderID = orderId;
            PermId = permId;
            Account = account;
            Symbol = symbol + " " + sectype;
            // SecType = sectype;
            Status = status;
            // Filled = filled;
            LimitPrice = limitPrice;
            //Price = price;
            //Size = size;
            TotalQnt = totalQnt;
            //OrderState = orderState;
        }

        //public int OrderID { get; set; }
        public int PermId { get; set; }
        public string Account { get; set; }
        public string Symbol { get; set; }
        //public string SecType { get; set; }
        public string Status { get; set; }
        //public int Filled { get; set; }
        //public double Price { get; set; }
        public int TotalQnt { get; set; }
        public double LimitPrice { get; set; }
    }
}