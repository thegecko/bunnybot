using System;
using System.Collections.Generic;
using System.ServiceProcess;
using org.theGecko.BunnyBot;
using org.theGecko.Utilities;

namespace org.theGecko.BunnyBot_Service
{
	class Program
	{
		static void Main(string[] args)
		{
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
