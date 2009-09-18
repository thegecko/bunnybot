namespace org.theGecko.BunnyBot_Service
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BunnyBotProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.BunnyBotInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// BunnyBotProcessInstaller
			// 
			this.BunnyBotProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.BunnyBotProcessInstaller.Password = null;
			this.BunnyBotProcessInstaller.Username = null;
			// 
			// BunnyBotInstaller
			// 
			this.BunnyBotInstaller.Description = "Nabaztag messenger bot";
			this.BunnyBotInstaller.DisplayName = "BunnyBot";
			this.BunnyBotInstaller.ServiceName = "BunnyBot";
			this.BunnyBotInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			this.BunnyBotInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.BunnyBotProcessInstaller,
            this.BunnyBotInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller BunnyBotProcessInstaller;
		private System.ServiceProcess.ServiceInstaller BunnyBotInstaller;
	}
}