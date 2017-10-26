using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace TcpServer
{
    [RunInstaller(true)]
    public partial class TcpServiceInstaller : System.Configuration.Install.Installer
    {
        private ServiceInstaller serviceInstaller;

        public TcpServiceInstaller()
        {
            InitializeComponent();

            serviceInstaller = new ServiceInstaller();

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = " TcpServer";
            serviceInstaller.Description = "Tcp Server Service";

            var servicesDependedOn = new List<string> { "tcpip" };

            serviceInstaller.ServicesDependedOn = servicesDependedOn.ToArray();

            Installers.Add(serviceInstaller);
        }
    }
}
