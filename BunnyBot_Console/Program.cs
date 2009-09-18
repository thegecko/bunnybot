using System;
using System.Collections.Generic;
using org.theGecko.BunnyBot;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot_Console
{
	class Program
	{
		static void Main(string[] args)
		{
			using (BunnyMessenger bunny = new BunnyMessenger(SettingsUtil.Instance.GetSetting("SerialID"), SettingsUtil.Instance.GetSetting("TokenID"), SettingsUtil.Instance.GetSetting("MSNUsername"), SettingsUtil.Instance.GetSetting("MSNPassword")))
			{
				bunny.Message = SettingsUtil.Instance.GetSetting("MSNMessage");
				bunny.DefaultVoice = SettingsUtil.Instance.GetSetting("DefaultVoice", "UK-Penelope");
				bunny.Names = SettingsUtil.Instance.GetSetting("RandomNames").Split(',');
				bunny.Templates = new Dictionary<string, string>();

				string[] templates = SettingsUtil.Instance.GetSetting("MessageTemplates").Split(',');

				foreach (string template in templates)
				{
					bunny.Templates.Add(template, SettingsUtil.Instance.GetSetting(template + "Message"));
				}

				bunny.Start();

				Console.WriteLine("Press any key to exit");
				Console.ReadKey();

				bunny.Stop();
			}
		}
	}
}
