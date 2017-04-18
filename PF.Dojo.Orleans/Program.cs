using System;
using Orleans;
using Orleans.Runtime.Configuration;

namespace PF.Dojo.Orleans
{
    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        private static OrleansHostWrapper hostWrapper;

        public static int Main(string[] args)
        {
            Console.WriteLine("Initializing Silo host...");
            int exitCode = StartSilo(args);
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

            hostWrapper = new OrleansHostWrapper(config, args);
            return hostWrapper.Run();
        }

        private static int ShutdownSilo()
        {
            return hostWrapper?.Stop() ?? 0;
        }
    }
}