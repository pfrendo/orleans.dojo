using System;
using Orleans.Runtime.Configuration;
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