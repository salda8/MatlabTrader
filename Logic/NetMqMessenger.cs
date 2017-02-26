using MATLAB_trader.Data;
using MATLAB_trader.Data.DataType;
using NetMQ;
using NetMQ.Sockets;
using QDMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Configuration;


namespace MATLAB_trader.Logic
{
    //THIS IS THE COOL PART OF THIS APPLICATION NET MQ
    public class NetMqMessenger
    {
        private readonly string pushConnectionString;
        private readonly object pushSocketLock = new object();
        private readonly object dealerSocketLock = new object();

        private PushSocket pushSocket;
        private DealerSocket dealerSocket;
        private readonly string dealerConnectionString;
        private static bool finished;
        private readonly int waitTimeBeforeEquityRequestInMs;
        private NetMQPoller poller;


        public NetMqMessenger()
        {
            var pushPort = Properties.Settings.Default.PushPort;
            if (pushPort>0)
            {
                pushConnectionString = $"tcp://localhost:{5700}";
            }
            else
            {
                throw new Exception("PushPort must be greater than zero.");
            }
           
            var defaultDealerPort = Properties.Settings.Default.DealerPort;
            if (defaultDealerPort > 0)
            {
                dealerConnectionString = $"tcp://localhost:{5556}";
            }
            else
            {
                throw new Exception("DealerPort must be greater than zero.");
            }

            waitTimeBeforeEquityRequestInMs=Properties.Settings.Default.WaitTimeBeforeEquityRequestInMs;
            



        }

        public void StartServerToUpdateEquity()
        {
            using (var sender = new DealerSocket())
            {
                sender.Connect(dealerConnectionString);
                Console.WriteLine($"Connected to {dealerConnectionString}");
                while (!finished)
                {
                    var message = new NetMQMessage();
                    message.AppendEmptyFrame();
                    message.Append(Program.AccountID);
                    sender.SendMultipartMessage(message);
                    Console.WriteLine($"Sent request with identity. {sender.Options.Identity}");
                    Console.WriteLine("Waiting for answer.");
                    var receiveFrameBytes = sender.ReceiveMultipartMessage();
                    Console.WriteLine("Answer received.");
                    using (var ms = new MemoryStream())
                    {
                        var equity = MyUtils.ProtoBufDeserialize<Equity>(receiveFrameBytes[1].Buffer, ms);
                        Console.WriteLine($"Equity: {equity.Value}");
                    }


                    Thread.Sleep(TimeSpan.FromMilliseconds(waitTimeBeforeEquityRequestInMs));
                }
            }
        }

        public void StartPushServer()
        {
            lock (pushSocketLock)
            {
                pushSocket = new PushSocket(pushConnectionString);
            }
           
            poller = new NetMQPoller { pushSocket };
            poller.RunAsync();
        }

        private void DealerSocketReceiveReadyHandler(object sender, NetMQSocketEventArgs e)
        {
            
        }

        public void StopServers()
        {
            StopPushServer();
            StopDealerServer();
        }

        private void StopDealerServer()
        {
            lock (pushSocketLock)
            {
                if (pushSocket != null)
                {
                    try
                    {
                        pushSocket.Disconnect(pushConnectionString);
                    }
                    finally
                    {
                        finished = true;
                        pushSocket.Close();
                        pushSocket = null;
                    }
                }
            }
        }

        private void StopPushServer()
        {
            lock (pushSocketLock)
            {
                if (pushSocket != null)
                {
                    try
                    {
                        pushSocket.Disconnect(pushConnectionString);
                        
                    }
                    finally
                    {
                        dealerSocket.ReceiveReady -= DealerSocketReceiveReadyHandler;
                        pushSocket.Close();
                        pushSocket = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the live contracts.
        /// </summary>
        /// <param name="result">The result.</param>
        public static void GetLiveContracts(IReadOnlyList<Portfolio> result)
        {
            foreach (var t in result)
            {
                //if (t.Symbol == "EUR")
                //{
                //    Matlab.EurContracts = t.Contracts;
                //}
                //else if (t.Symbol == "GBP")
                //{
                //    Matlab.GbpContracts = t.Contracts;
                //}
                //else if (t.Symbol == "JPY")
                //{
                //    Matlab.JpyContracts = t.Contracts;
                //}
                //else if (t.Symbol == "AUD")
                //{
                //    Matlab.AudContracts = t.Contracts;
                //}
                //else if (t.Symbol == "CAD")
                //{
                //    Matlab.CadContracts = t.Contracts;
                //}
            }
        }

        /// <summary>
        ///     Selects the trades from portfolio table in database.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public static void SelectTradesFromDatabase(string accountNumber)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT * FROM Portfolio WHERE Account=?acc";
                    cmd.Parameters.AddWithValue("?acc", accountNumber);

                    var dt = new DataTable(@"PortfolioTable");

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    var result =
                        Enumerable.Select(dt.AsEnumerable(),
                            row => new Portfolio(row[1].ToString(), row[2].ToString(), row[8].ToString()))
                            .ToList();
                    Portfolio.LivePortfolioTradesList.AddRange(result);
                    if (result.Count > 0)
                    {
                        GetLiveContracts(result);
                    }
                }
                //con.Close();
            }
        }

        /// <summary>
        ///     Handles the error MSG.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMsg">The error MSG.</param>
        /// <param name="acc"></param>
        public static void HandleErrorMsg(int errorCode, string errorMsg, string acc)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO Errors (ErrorCode, ErrorMessage, AccountNumber) VALUES (?ec, ?em, ?acc)";
                    cmd.Parameters.AddWithValue("?acc", acc);
                    cmd.Parameters.AddWithValue("?em", errorMsg);
                    cmd.Parameters.AddWithValue("?ec", errorCode);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        #region MessageHandling

        public static TradeDirection ConvertFromString(string side) => side == "BUY" ? TradeDirection.Long : TradeDirection.Short;

        public void HandleMessages(object objectToSend, GeneralRequestMessageType messageType)
        {
            using (var ms = new MemoryStream())
            {
                var messageToSend = new NetMQMessage(2);
                messageToSend.Append(BitConverter.GetBytes((byte) messageType));
                messageToSend.Append(MyUtils.ProtoBufSerialize(objectToSend, ms));
                pushSocket.SendMultipartMessage(messageToSend);
                  
            }
        }

        #endregion MessageHandling

        //// <summary>
        //// Looks OBSOLETE
        ////     Selects the open orders from database.
        //// </summary>
        //// <param name="accountNumber">The account number.</param>
        //public static void SelectOpenOrdersFromDatabase(string accountNumber)
        //{
        //}

        //// <summary>
        //// LOOKS OBSOLETE
        ////     Selects the live trade from database.
        //// </summary>
        //// <param name="accountNumber">The account number.</param>
        //public static void SelectLiveTradeFromDatabase(string accountNumber)
        //{
        //
        //}
    }
}