using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public partial class TcpServer : ServiceBase
    {
        public static IOTcpServer s_tcpServer = new IOTcpServer();
        public static ManualResetEvent _isShutDown;

        public TcpServer()
        {
            InitializeComponent();
            _isShutDown = new ManualResetEvent(false);
        }

        static void Main(string[] args)
        {
            bool bDebug = false;
            if (0 < args.Length)
            {
                string exeArg = args[0];

                switch (exeArg.ToLower())
                {
                    case ("-i"):
                    case ("-install"):
                        Install();
                        return;

                    case ("-u"):
                    case ("-uninstall"):
                        Uninstall();
                        return;

                    case ("-d"):
                    case ("-debug"):
                        bDebug = true;
                        break;

                    default:
                        Console.WriteLine("Invalid argument!");
                        break;
                }
            }

            if(true == bDebug)
            {

                RunAsConsole();
            }
            else
            {
                RunAsService();
            }

        }

        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


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

        static void RunAsConsole()
        {
            TcpServer server = new TcpServer();
            server.OnStart(null);
            server.OnStop();
            Environment.Exit(0);
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Try to start TcpServer!");
            Console.WriteLine("-------------------------------------------------------------------");

            if(false == UserSocket.InitializePacketHandler())
            {
                Console.WriteLine("InitializePacketHandler execution failed");
                return;
            }

            int port = 20000;
            s_tcpServer.Start(port);

            string input = string.Empty;
            while(true)
            {
                input = Console.ReadLine();
                if (0 == input.ToLower().CompareTo("quit"))
                    break;
            }
        }

        protected override void OnStop()
        {
            s_tcpServer.Stop();
            Console.WriteLine("TcpServer stopped!");
        }
    }
}
