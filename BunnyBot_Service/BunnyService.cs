using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot
{
	public partial class BunnyService : ServiceBase
	{
		private BunnyMessenger _bunny;
		private Thread _workerThread;
		private readonly int _sleepTime;
		
		public BunnyService()
		{
			InitializeComponent();
			_sleepTime = SettingsUtil.Instance.GetSetting("SleepTime", 30000);
		}

		public void StartService()
		{
			_bunny = new BunnyMessenger(SettingsUtil.Instance.GetSetting("SerialID"),
			                                          SettingsUtil.Instance.GetSetting("TokenID"),
			                                          SettingsUtil.Instance.GetSetting("MSNUsername"),
			                                          SettingsUtil.Instance.GetSetting("MSNPassword"))
           		{
           			Message = SettingsUtil.Instance.GetSetting("MSNMessage"),
           			DefaultVoice = SettingsUtil.Instance.GetSetting("DefaultVoice", "UK-Penelope"),
           			Templates = new Dictionary<string, string>()
           		};

			string[] templates = SettingsUtil.Instance.GetSetting("MessageTemplates").Split(',');

			foreach (string template in templates)
			{
				_bunny.Templates.Add(template, SettingsUtil.Instance.GetSetting(template + "Message"));
			}

			_workerThread = new Thread(_bunny.StartThread);
			_workerThread.Start();
		}

		public void StopService()
		{
			_bunny.StopThread();
			_workerThread.Join(60000);

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
