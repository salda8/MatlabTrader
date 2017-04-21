using IBApi;
using NDesk.Options;
using NLog;
using NLog.Targets;
using StrategyTrader.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace StrategyTrader
{
    public class Program
    {
        public static int AccountID = Settings.Default.AccountID;
        private static IbClient wrapper;

        public static void Main(string[] args)
        {
            SetAndCreateLogDirectory();
            MappingConfiguration.Register();
            GetAccountNumberAndAccountID();
            ConnectToIb();
            new StrategyLauncher(wrapper);
        }

        private static void SetAndCreateLogDirectory()
        {
            if (Directory.Exists(Settings.Default.logDirectory))
            {
                ((FileTarget)LogManager.Configuration.FindTargetByName("logfile")).FileName =
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
                Settings.Default.EquityUpdateServerRouterPort, Settings.Default.MessagesServerPullPort, Settings.Default.AccountNumber);
            client.Connect();
            
            

            wrapper = new IbClient(client);
            EClientSocket clientSocket = wrapper.ClientSocket;
            EReaderSignal readerSignal = wrapper.Signal;

            clientSocket.eConnect(Settings.Default.IBGatewayIP, Settings.Default.ibPort, 0);
           
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
            
           
            
            
        }

        private static void GetAccountNumberAndAccountID()
        {
            var requestClient = new RequestsClient.RequestsClient(Settings.Default.AccountID,
                Settings.Default.InstrumetnUpdateRequestSocketPort);
            var acc = requestClient.RequestAccount(Settings.Default.StrategyID);
            if (acc == null)
            {
                throw new ArgumentNullException("No account info received.");
            }
            requestClient.Dispose();

            Settings.Default.AccountNumber = acc.AccountNumber;
            Settings.Default.AccountID = acc.ID;
            Settings.Default.Save();
        }

        

        
    }
}