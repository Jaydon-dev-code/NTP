using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.serviceInstaller1.Description = "双林数据追溯后台服务。";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;  // 自动启动
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceInstaller1_AfterInstall_1(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterInstall_1(object sender, InstallEventArgs e)
        {

        }
    }
}
