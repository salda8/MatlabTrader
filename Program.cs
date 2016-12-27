using System;
using System.Collections.Generic;
using MATLAB_trader.Data;
using MATLAB_trader.Logic;
using NDesk.Options;

namespace MATLAB_trader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var showHelp = false;
            var names = new List<string>();
            var port = new List<int>();
            var account = new List<string>();
            var repeat = 1;

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
                {
                    "m|matlab=", "the {account} of someone to greet.",
                    v => Matlab.Matlabexe = v
                },

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
                    Console.WriteLine("Using Account number: " + account[i] + " Port:" + port[i] +
                                      " on this matlab function:" + Matlab.Matlabexe);
                }
            }

            Matlab.StartTrading();

            //Console.ReadKey();
            //return 0;
        }
    }
}