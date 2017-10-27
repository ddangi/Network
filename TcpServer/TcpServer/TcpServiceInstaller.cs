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
        private ServiceProcessInstaller serviceProcessInstaller;

        public TcpServiceInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.DisplayName = " TcpServer";
            serviceInstaller.ServiceName = "TcpServer";
            serviceInstaller.Description = "Tcp Server Service";
            serviceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(serviceInstaller_AfterInstall);

            var servicesDependedOn = new List<string> { "tcpip" };

            serviceInstaller.ServicesDependedOn = servicesDependedOn.ToArray();

            Installers.AddRange(new System.Configuration.Install.Installer[] {
            serviceProcessInstaller,
            serviceInstaller});
        }

        private void serviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName, Environment.MachineName))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                        sc.Start();
                }
            }
            catch (Exception ee)
            {
                //EventLog.WriteEntry("Application", ee.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
