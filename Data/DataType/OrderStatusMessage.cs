using System.Collections.Generic;

namespace MATLAB_trader.Data.DataType
{
    public class OrderStatusMessage
    {
        public static List<OrderStatusMessage> OrderStatusMessageList = new List<OrderStatusMessage>();

        public OrderStatusMessage(int orderId, string status, int filled, int remaining, double avgFillPrice,
            int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            OrderId = orderId;
            Status = status;
            Filled = filled;
            Remaining = remaining;
            AvgFillPrice = avgFillPrice;
            PermId = permId;
            ParentId = parentId;
            LastFillPrice = lastFillPrice;
            ClientId = clientId;
            WhyHeld = whyHeld;
        }

        public int OrderId { get; set; }
        public string Status { get; set; }
        public int Filled { get; set; }
        public int Remaining { get; set; }
        public double AvgFillPrice { get; set; }
        public int PermId { get; set; }
        public int ParentId { get; set; }
        public double LastFillPrice { get; set; }
        public int ClientId { get; set; }
        public string WhyHeld { get; set; }
    }
}