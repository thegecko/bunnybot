using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

namespace org.theGecko.BunnyBot_Service
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
            this.BeforeInstall += Installer_Handler;
            this.BeforeUninstall += Installer_Handler; 
		}

        private void Installer_Handler(object sender, InstallEventArgs e)
		{
            if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
            {
                this.BunnyBotInstaller.ServiceName = this.BunnyBotInstaller.DisplayName = this.Context.Parameters["ServiceName"];
            } 
		}
	}
}
