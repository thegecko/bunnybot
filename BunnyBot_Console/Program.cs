using System;
using log4net.Config;
using org.theGecko.BunnyBot;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot_Console
{
	class Program
	{
		static void Main(string[] args)
		{
            XmlConfigurator.Configure();

			using (BunnyMessenger bunny = new BunnyMessenger(SettingsUtil.Instance.GetSetting("SerialID"), SettingsUtil.Instance.GetSetting("TokenID"), SettingsUtil.Instance.GetSetting("MSNUsername"), SettingsUtil.Instance.GetSetting("MSNPassword")))
			{
				bunny.Start();

				Console.WriteLine("Press any key to exit");
				Console.ReadKey();

				bunny.Stop();
			}
		}
	}
}
