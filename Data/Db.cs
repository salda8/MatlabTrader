using System;
using System.Data.SqlClient;


namespace MATLAB_trader.Data
{
    public class Db
    {
        public static string ConnectionString = @"(localdb)\MSSQLLocalDB";

        /// <summary>
        ///     Opens the connection.
        /// </summary>
        /// <returns></returns>
        public static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
            }
            catch (SqlException exception)
            {
                Console.WriteLine(exception.Message);
            }

            return connection;
        }

        /// <summary>
        ///     Inserts the istrade variable in database.
        /// </summary>
        /// <param name="trade">if set to <c>true</c> [trade].</param>
        public static void InsertTadeInDb(bool trade)
        {
            using (var con = OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE IsTrade SET value=?trade";
                    cmd.Parameters.AddWithValue("?trade", trade);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        ///     Gets the trade variable on start up.
        /// </summary>
        /// <returns></returns>
        public static bool GetTradeVariableOnStartUp()
        {
            var istrade = false;
            using (var con = OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT * FROM IsTrade";

                    using (var reader = cmd.ExecuteReader())

                    {
                        while (reader.Read())
                        {
                            istrade = reader.GetBoolean(1);
                        }
                    }
                    return istrade;
                }
            }
        }
    }
}