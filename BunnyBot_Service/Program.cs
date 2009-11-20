using System;
using System.ServiceProcess;
using log4net.Config;
using org.theGecko.BunnyBot;

namespace org.theGecko.BunnyBot_Service
{
	class Program
	{
		static void Main(string[] args)
		{
            XmlConfigurator.Configure();

			BunnyService bunnyService = new BunnyService();

			if (Environment.UserInteractive)
			{
				bunnyService.StartService();

				Console.WriteLine("Press any key to exit");
				Console.ReadKey();
			
				bunnyService.StopService();
			}
			else
			{
				ServiceBase[] services = new ServiceBase[]
                 	{
                 		bunnyService
                 	};

				ServiceBase.Run(services);
			}
		}
	}
}
