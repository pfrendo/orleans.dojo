using System;
using System.Collections.Generic;
using Orleans.Runtime.Configuration;
using Orleans.Storage;
using PF.Dojo.Silo;

namespace PF.Dojo.Orleans
{
	public class Program
	{
		private static OrleansHostWrapper _hostWrapper;

		public static int Main(string[] args)
		{
			Console.WriteLine("Initializing Silo host...");
			var exitCode = StartSilo(args);
			Console.WriteLine("Press any key to terminate.");
			Console.ReadLine();

			exitCode += ShutdownSilo();

			return exitCode;
		}

		private static int StartSilo(string[] args)
		{
			var config = ClusterConfiguration.LocalhostPrimarySilo();
			config.LoadFromFile(@".\OrleansConfiguration.xml");
			var props = new Dictionary<string, string>
			{
				["DataConnectionString"] = "Service=eu-west-1;AccessKey=<ACCESS KEY HERE>;SecretKey=<SECRET KEY HERE>",
				["TableName"] = "UserGrainState",
				["UseJsonFormat"] = "true"
			};
			config.Globals.RegisterStorageProvider<DynamoDBStorageProvider>("DDBStore", props);
			config.AddMemoryStorageProvider();
			//config.Defaults.DefaultTraceLevel = Severity.Verbose3;

			_hostWrapper = new OrleansHostWrapper(config, args);
			return _hostWrapper.Run();
		}

		private static int ShutdownSilo()
		{
			return _hostWrapper?.Stop() ?? 0;
		}
	}
}