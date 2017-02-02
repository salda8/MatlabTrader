using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using MATLAB_trader.Data;
using MATLAB_trader.Data.DataType;
using MATLAB_trader.Logic;
using NDesk.Options;
using System.Configuration;
using QDMS;

namespace MATLAB_trader
{
    public class Program
    {
        public static int AccountID=1;
        private static IbClient wrapper;

        public static void Main(string[] args)
        {
            var showHelp = false;
            var names = new List<string>();
            var port = new List<int>();
            var account = new List<string>();
           

            var p = new OptionSet
            {
                {
                    "n|name=", "the {NAME} of someone to greet.",
                    v => names.Add(v)
                },
                {
                    "p|port=", "the {port} of someone to greet.",
                    (int v) => port.Add(v)
                },
                {
                    "a|account=", "the {account} of someone to greet.",
                    v => account.Add(v)
                },
                //{
                //    //"m|matlab=", "the {account} of someone to greet.",
                //    ////v => Matlab.Matlabexe = v
                //},

                {
                    "h|help", "show this message and exit",
                    v => showHelp = v != null
                }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return;
            }

            if (showHelp)
            {
                // ShowHelp(p);
                return;
            }

            if (extra.Count > 0)
            {
                var message = string.Join(" ", extra.ToArray());
                Console.WriteLine
                    (" At least one Unrecognized parameter. Using new message: {0}", message);
            }
            else
            {
                for (var i = 0; i < account.Count; i++)
                {
                    AccountSettings.AccountSettingsList.Add(new AccountSettings
                    {
                        AccountNumber = account[i],
                        Port = port[i]
                    });
                    //Console.WriteLine("Using Account number: " + account[i] + " Port:" + port[i] +
                    //                  " on this matlab function:" + Matlab.Matlabexe);
                }
            }

            StartTrading();
            //Matlab.StartTrading();

            //Console.ReadKey();
            //return 0;
        }

        public static void StartTrading()
        {

            ConnectToIb();
            
            Thread.Sleep(1000);

            // var ml = new Matlab();

            while (true)
            {
                //Trade.PlaceTrade(MyContracts.Contract(), 1, wrapper);
                //while (TradingCalendar.TradingDay())
                //{
                //    while (HighResolutionDateTime.UtcNow.Second != 0)
                //    {
                //        Thread.Sleep(1);

                //    }

                //    Trade.PlaceTrade(MyContracts.Contract(), 1, wrapper);
                //}
                //Thread.Sleep(10000);
            }
        }

        

        private static void ConnectToIb()
        {
            var orderManager = new OrderManager();
            orderManager.StartPushServer();
            Task.Factory.StartNew(orderManager.StartServerToUpdateEquity, TaskCreationOptions.LongRunning);

            //wrapper = new IbClient(orderManager);
            //EClientSocket clientSocket = wrapper.ClientSocket;
            //EReaderSignal readerSignal = wrapper.Signal;
            
            //clientSocket.eConnect("127.0.0.1", 7496, 0);
            //clientSocket.reqAllOpenOrders();
            //clientSocket.reqPositions();
          
            ////Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
            //var reader = new EReader(clientSocket, readerSignal);
            //reader.Start();
            ////Once the messages are in the queue, an additional thread need to fetch them
            //new Thread(() =>
            //{
            //    while (clientSocket.IsConnected())
            //    {
            //        readerSignal.waitForSignal();
            //        reader.processMsgs();
            //    }
            //}) {IsBackground = true}.Start();

            //while (wrapper.NextOrderId <= 0) { }
        }

        public static int LoadedSymbolInstrumentID(string messageContractSymbol)
        {
            return LoadedSymbolsDictionary[messageContractSymbol];
        }
        public static Dictionary<string, int> LoadedSymbolsDictionary { get; set; }
    }
}