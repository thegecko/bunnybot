using System;
using System.Threading;
using log4net;
using log4net.Config;
using org.theGecko.BunnyBot;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot_Console
{
	class Program
	{
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
            XmlConfigurator.Configure();

			using (BunnyMessenger bunny = new BunnyMessenger(
                SettingsUtil.Cached.GetSetting("SerialID"),
                SettingsUtil.Cached.GetSetting("TokenID"),
                SettingsUtil.Cached.GetSetting("MSNUsername"),
                SettingsUtil.Cached.GetSetting("MSNPassword")))
			{
                bunny.MsnImagePath = SettingsUtil.Instance.GetSetting("MsnImage");

                Thread workerThread = new Thread(bunny.Start);
                workerThread.Start();

                Log.Info("Press any key to exit");
				Console.ReadKey();

                bunny.Stop();
                workerThread.Join(10000);
			}
		}
	}
}
