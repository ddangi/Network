using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    static class Program
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (0 < args.Length)
            {
                string exeArg = args[1];

                switch (exeArg.ToLower())
                {
                    case ("-i"):
                    case ("-install"):
                        Install();
                        break;

                    case ("-u"):
                    case ("-uninstall"):
                        Uninstall();
                        break;

                    case ("-d"):
                    case ("-debug"):
                        RunAsConsole();
                        break;

                    default:
                        Console.WriteLine("Invalid argument!");
                        break;
                }
            }
            else
            {
                RunAsService();
            }
        }

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (setConsoleColor)
                Console.ForegroundColor = color;
        }

        private static void Install()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { _exePath });
            }
            catch
            {
                Console.WriteLine("failed to install Service!");
            }
        }

        private static void Uninstall()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", _exePath });
            }
            catch
            {
                Console.WriteLine("failed to install Service!");
            }
        }

        private static void RunAsService()
        {
            ServiceBase[] servicesToRun;

            servicesToRun = new ServiceBase[] { new TcpServer() };

            ServiceBase.Run(servicesToRun);
        }

        private static bool setConsoleColor;

        private static void SetConsoleColor(/*LogLevel*/)
        {
            SetConsoleColor(ConsoleColor.Red);
            SetConsoleColor(ConsoleColor.Green);
        }

        static void RunAsConsole()
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Try to start TcpServer on console mode!");
            Console.WriteLine("-------------------------------------------------------------------");


            Console.WriteLine("-------------------------------------------------------------------");
        }
    }
}
