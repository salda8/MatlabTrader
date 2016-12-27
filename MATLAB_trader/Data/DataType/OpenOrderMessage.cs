using System;
using System.Collections.Generic;
using IBApi;

namespace MATLAB_trader.Data.DataType
{
    public class OpenOrderMessage
    {
        public static List<OpenOrderMessage> OpenOrderMessageList = new List<OpenOrderMessage>();
        private string _v10;
        private string _v2;
        private string _v3;
        private string _v4;
        private string _v5;
        private string _v6;
        private string _v7;
        private string _v8;
        private string _v9;

        public OpenOrderMessage(int orderId, Contract contract, Order order, OrderState orderState)
        {
            PermId = order.PermId;
            Account = order.Account;

            ContractSymbol = contract.Symbol + contract.SecType;
            //Contract = contract.SecType;
            Side = order.Action;
            OrderState = orderState.Status;
            Commissision = orderState.Commission;
            LimitPrice = order.LmtPrice;
            Qty = order.TotalQuantity;
            Type = order.OrderType;
            Tif = order.Tif;

            //Size = order.DisplaySize;
        }

        public OpenOrderMessage(int permid, string symbol, string orderstate, double limitprice, int qty, string account,
            string side, string time, string type, string tif)
        {
            PermId = permid;
            Account = account;
            ContractSymbol = symbol;
            OrderState = orderstate;
            Side = side;
            Tif = tif;
            Type = type;
            LimitPrice = limitprice;
            Qty = qty;
        }

        public OpenOrderMessage(string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8,
            string v9, string v10)
        {
            PermId = Convert.ToInt32(v1);
            _v2 = v2;
            _v3 = v3;
            _v4 = v4;
            _v5 = v5;
            _v6 = v6;
            _v7 = v7;
            _v8 = v8;
            _v9 = v9;
            _v10 = v10;
        }

        public OpenOrderMessage()
        {
        }

        public string Side { get; set; }
        public string Type { get; set; }
        public string Account { get; set; }
        public int PermId { get; set; }
        public string ContractSymbol { get; set; }
        //public string Contract { get; set; }
        //public int Size { get; set; }
        public double LimitPrice { get; set; }
        public int Qty { get; set; }
        public string OrderState { get; set; }
        public double Commissision { get; set; }
        public string Tif { get; set; }
    }
}