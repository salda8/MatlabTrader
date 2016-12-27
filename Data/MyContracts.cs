using System;
using System.Collections.Generic;
using IBApi;

namespace MATLAB_trader.Data
{
    public class MyContracts
    {
        public static List<Contract> ContractsList = new List<Contract>();
        public static List<ContractExt> ContractsExtList = new List<ContractExt>();
        public static List<ContractExt> ContractsExtInUseList = new List<ContractExt>();
        public static List<int> TickerIdList = new List<int>();

        /// <summary>
        ///     Contracts the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="secType">Type of the sec.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="exchange">The exchange.</param>
        /// <param name="localSymbol">The local symbol.</param>
        /// <returns></returns>
        public static Contract Contract(string symbol = "GC", string secType = "FUT", string currency = "USD",
            string exchange = "NYMEX", string localSymbol = "GCQ5")
        {
            var contract = new Contract
            {
                Symbol = symbol,
                SecType = secType,
                Currency = currency,
                Exchange = exchange,
                LocalSymbol = localSymbol
            };

            return contract;
        }

        /// <summary>
        ///     Gets all contracts.
        /// </summary>
        public static void GetAllContracts()
        {
            if (ContractsList.Count > 0)
            {
                ContractsList.Clear();
            }
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT * FROM Contracts";


                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null && reader.GetString(1) != null && reader.GetString(2) != null &&
                            reader.GetString(3) != null && reader.GetString(4) != null && reader.GetString(5) != null)
                        {
                            ContractsList.Add(new Contract
                            {
                                Symbol = reader.GetString(1),
                                SecType = reader.GetString(2),
                                Currency = reader.GetString(3),
                                Exchange = reader.GetString(4),
                                LocalSymbol = reader.GetString(5)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all contracts.
        /// </summary>
        public static void GetAllExtContracts()
        {
            if (ContractsList.Count > 0)
            {
                ContractsList.Clear();
            }
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT * FROM contracts_ext";


                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null && reader.GetString(1) != null && reader.GetString(2) != null &&
                            reader.GetString(3) != null && reader.GetString(4) != null && reader.GetString(5) != null &&
                            reader.GetString(6) != null)
                        {
                            ContractsExtList.Add(new ContractExt
                            {
                                Symbol = reader.GetString(1),
                                SecType = reader.GetString(2),
                                Currency = reader.GetString(3),
                                Exchange = reader.GetString(4),
                                LocalSymbol = reader.GetString(5),
                                RolloverDate = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all contracts.
        /// </summary>
        public static void GetExtContractsFromDb(string symbol)
        {
            if (ContractsExtInUseList.Count > 0)
            {
                ContractsExtInUseList.Clear();
            }
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT * FROM contracts_ext WHERE Symbol=?s";
                cmd.Parameters.AddWithValue("?s", symbol);


                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null && reader.GetString(1) != null && reader.GetString(2) != null &&
                            reader.GetString(3) != null && reader.GetString(4) != null && reader.GetString(5) != null &&
                            reader.GetString(6) != null)
                        {
                            ContractsExtInUseList.Add(new ContractExt
                            {
                                Symbol = reader.GetString(1),
                                SecType = reader.GetString(2),
                                Currency = reader.GetString(3),
                                Exchange = reader.GetString(4),
                                LocalSymbol = reader.GetString(5),
                                RolloverDate = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
        }

        //public static void GetTickerId(string symbol)
        //{
        //    using (var con = Db.OpenConnection())
        //    {
        //        var cmd = con.CreateCommand();
        //        cmd.CommandText =
        //            "SELECT Distinct(TickerId) FROM Contracts_ext WHERE Symbol=?s";
        //        cmd.Parameters.AddWithValue("?s", symbol);

        //        using (var reader = cmd.ExecuteReader())

        //        {
        //            while (reader.Read())
        //            {

        //                TickerIdList.Add(reader.GetInt32(0));
        //                GetContractsFromDbOnStartup(reader.GetInt32(0));

        //            }
        //        }
        //    }
        //}

        /// <summary>
        ///     Gets all contracts.
        /// </summary>
        public static void GetContractsFromDbOnStartup(string tickerId)
        {
            //if (ContractsList.Count > 0)
            //{
            //    ContractsList.Clear();
            //}
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT * FROM contracts WHERE cc = ?ticker AND Rollover_Date>?date ORDER BY contracts_id ASC LIMIT 1";
                cmd.Parameters.AddWithValue("?ticker", tickerId);
                cmd.Parameters.AddWithValue("?date", DateTime.UtcNow.Date);

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null && reader.GetString(1) != null && reader.GetString(2) != null &&
                            reader.GetString(3) != null && reader.GetString(4) != null && reader.GetString(5) != null &&
                            reader.GetString(6) != null && reader.GetString(7) != null)
                        {
                            var productcode = "";
                            switch (reader.GetString(1))
                            {
                                case "EUR":
                                    productcode = "6EZ5";
                                    break;
                                case "GBP":
                                    productcode = "6BZ5";
                                    break;
                                case "JPY":
                                    productcode = "6JZ5";
                                    break;
                                case "AUD":
                                    productcode = "6AZ5";
                                    break;
                                case "CAD":
                                    productcode = "6CZ5";
                                    break;
                            }


                            var contract = new ContractExt
                            {
                                Symbol = reader.GetString(1),
                                SecType = "FUT",
                                Currency = "USD",
                                Exchange = reader.GetString(4),
                                LocalSymbol = productcode,
                                RolloverDate = reader.GetDateTime(7),
                                TickerId = reader.GetString(6)
                            };


                            ContractsExtList.Add(contract);

                            //Console.WriteLine(contract.Symbol + " | "+contract.TickerId +" | "+ contract.LocalSymbol+ " | "+contract.RolloverDate + " | " +contract.SecType + " | " + contract.Exchange);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the contract.
        /// </summary>
        /// <param name="getString">The get string.</param>
        /// <returns></returns>
        public static ContractExt GetContract(string getString)
        {
            getString = getString.Substring(0, 3);
            var xxx = ContractsExtList.FindIndex((x => x.Symbol == getString));

            return ContractsExtList[xxx];
        }

        public static Contract GetExtContract(string getString)
        {
            getString = getString.Substring(0, 3);
            var xxx = ContractsExtInUseList.FindAll((x => x.Symbol == getString));
            var xxxx = xxx.FindIndex((x => x.RolloverDate > DateTime.UtcNow));

            return ContractsExtInUseList[xxxx];
        }

        public static Contract GetExtContractOnRolloverDay(string getString)
        {
            getString = getString.Substring(0, 3);
            var xxx = ContractsExtInUseList.FindAll((x => x.Symbol == getString));
            var xxxx = xxx.FindIndex((x => x.RolloverDate.ToString("d") == DateTime.UtcNow.ToString("d")));

            return ContractsExtInUseList[xxxx];
        }
    }

    public class ContractExt : Contract
    {
        public DateTime RolloverDate { get; set; }
        public string TickerId { get; set; }
    }
}