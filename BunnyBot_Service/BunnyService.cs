﻿using System.ServiceProcess;
using System.Threading;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot
{
	public partial class BunnyService : ServiceBase
	{
		private BunnyMessenger _bunny;
		private Thread _workerThread;
		
		public BunnyService()
		{
			InitializeComponent();
		}

		public void StartService()
		{
			_bunny = new BunnyMessenger(
                SettingsUtil.Cached.GetSetting("SerialID"),
                SettingsUtil.Cached.GetSetting("TokenID"),
                SettingsUtil.Cached.GetSetting("MSNUsername"),
                SettingsUtil.Cached.GetSetting("MSNPassword"));

            _bunny.MsnImagePath = SettingsUtil.Instance.GetSetting("MsnImage");

			_workerThread = new Thread(_bunny.Start);
			_workerThread.Start();
		}

		public void StopService()
		{
			_bunny.Stop();
			_workerThread.Join(10000);

			if (_bunny != null)
			{
				_bunny.Dispose();
			}
		}

		#region Windows Service Events

		protected override void OnStart(string[] args)
		{
			StartService();
		}

		protected override void OnStop()
		{
			StopService();
		}

		protected override void OnShutdown()
		{
			OnStop();
		}

		#endregion
	}
}
