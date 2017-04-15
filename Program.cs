using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using IBApi;
using NDesk.Options;
using NLog;
using NLog.Targets;
using StrategyTrader.Properties;

namespace StrategyTrader
{
    public class Program
    {
        public static int AccountID=Settings.Default.AccountID;
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
                //    ////v => Matlab.MatlabFunction = v
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
              
                Console.WriteLine(e.Message);
              
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
                    ("At least one Unrecognized parameter. Using new message: {0}", message);
            }
            
            SetAndCreateLogDirectory();
            MappingConfiguration.Register();
            ConnectToIb();
            new StrategyLauncher(wrapper);
        }

        private static void SetAndCreateLogDirectory()
        {
            if (Directory.Exists(Settings.Default.logDirectory))
            {
                ((FileTarget) LogManager.Configuration.FindTargetByName("logfile")).FileName =
                    Settings.Default.logDirectory + "Log.log";

            }
            else
            {
                Directory.CreateDirectory(Settings.Default.logDirectory);
                ((FileTarget)LogManager.Configuration.FindTargetByName("logfile")).FileName =
                    Settings.Default.logDirectory + "Log.log";
            }            
        }
        private static void ConnectToIb()
        {
            RequestsClient.RequestsClient client = new RequestsClient.RequestsClient(AccountID,
                Settings.Default.EquityUpdateServerRouterPort, Settings.Default.MessagesServerPullPort,  Settings.Default.AccountNumber);
            client.StartPushServer();

            wrapper = new IbClient(client);
            EClientSocket clientSocket = wrapper.ClientSocket;
            EReaderSignal readerSignal = wrapper.Signal;

            clientSocket.eConnect("127.0.0.1", Settings.Default.ibPort, 0);
            clientSocket.reqAllOpenOrders();
            clientSocket.reqPositions();
           
            //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread need to fetch them
            new Thread(() =>
            {
                while (clientSocket.IsConnected())
                {
                    readerSignal.waitForSignal();
                    reader.processMsgs();
                }
            })
            { IsBackground = true }.Start();

            while (wrapper.NextOrderId <= 0) { }

            clientSocket.reqGlobalCancel();
            clientSocket.reqAccountUpdates(true, Settings.Default.AccountNumber);


        }

        public static int LoadedSymbolInstrumentID(string messageContractSymbol)
        {
            return LoadedSymbolsDictionary[messageContractSymbol];
        }
        public static Dictionary<string, int> LoadedSymbolsDictionary { get; set; }
    }
}