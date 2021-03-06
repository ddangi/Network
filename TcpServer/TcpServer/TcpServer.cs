﻿using ServerBase;
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
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Dump.CurrentDomain_UnhandledException);

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("serverLog.xml"));

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
                        Log.log.Error("Invalid argument!");
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
                Log.log.Error("failed to install Service!");
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
                Log.log.Error("failed to install Service!");
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
            Log.log.Info("-------------------------------------------------------------------");
            Log.log.Info("Try to start TcpServer!");
            Log.log.Info("-------------------------------------------------------------------");

            if(false == ConfigManager.Instance.LoadServerConfig())
            {
                Log.log.Error("LoadServerConfig failed");
                return;
            }

            if (false == UserSocket.InitializePacketHandler())
            {
                Log.log.Error("InitializePacketHandler execution failed");
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
            Log.log.Info("TcpServer stopped!");
        }
    }
}
