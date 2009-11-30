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

			using (BunnyMessenger bunny = new BunnyMessenger(
                SettingsUtil.Cached.GetSetting("SerialID"),
                SettingsUtil.Cached.GetSetting("TokenID"),
                SettingsUtil.Cached.GetSetting("MSNUsername"),
                SettingsUtil.Cached.GetSetting("MSNPassword")))
			{
				bunny.Start();

				Console.WriteLine("Press any key to exit");
				Console.ReadKey();

				bunny.Stop();
			}
		}
	}
}
