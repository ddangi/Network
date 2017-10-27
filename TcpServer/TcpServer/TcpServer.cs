using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public partial class TcpServer : ServiceBase
    {
        public TcpServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Try to start TcpServer!");
            Console.WriteLine("-------------------------------------------------------------------");


            Console.WriteLine("-------------------------------------------------------------------");
        }

        protected override void OnStop()
        {
            Console.WriteLine("TcpServer stopped!");
        }
    }
}
