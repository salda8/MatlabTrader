using System;
using MATLAB_trader.Data;

namespace MATLAB_trader.Logic
{
    internal class TradingCalendar
    {
        public static bool TradingDay()
        {
            var tradingday = true;
            var dt = DateTime.Now;
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            {
                //Data.Clearticklists();
                tradingday = false;
            }
            return tradingday;
        }

        public static void RolloverContract(ContractExt contracts)
        {
        }

        public static void CheckRolloverDate()
        {
            foreach (var contracts in MyContracts.ContractsExtInUseList)
            {
                if (contracts.RolloverDate.ToString("d") == DateTime.UtcNow.ToString("d"))
                {
                    RolloverContract(contracts);
                }
            }
        }

        private void CloseTradesByContract(string symbol)
        {
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT Position FROM Portfolio WHERE Symbol=?symbol AND WHERE Account=?acc";
                cmd.Parameters.AddWithValue("?symbol", symbol);
                //cmd.Parameters.AddWithValue("?acc", Program.AccountNumber);


                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null && reader.GetString(0) != null && reader.GetString(0) != null)
                        {
                            //Trade.NewTrade(MyContracts.GetExtContract(symbol), reader.GetInt32(0));
                        }
                    }
                }
            }
        }
    }
}