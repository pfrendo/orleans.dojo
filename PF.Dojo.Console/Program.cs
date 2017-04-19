using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using PF.Dojo.User.Interfaces;

namespace PF.Dojo.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            System.Console.WriteLine("Initializing Console Client...");

            var config = ClientConfiguration.LocalhostSilo();

            try
            {
                InitializeWithRetries(config, 5);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Orleans client initialization failed failed due to {ex}");
                System.Console.ReadLine();
                return 1;
            }
            DoClientWork().Wait();
            System.Console.WriteLine("Press Enter to terminate...");
            System.Console.ReadLine();
            return 0;
            
        }

        private static async Task DoClientWork()
        {
            var user = GrainClient.GrainFactory.GetGrain<IUserGrain>(new Guid());

            var userDetails = new UserDetails();

            System.Console.WriteLine("Please provide a username!");
            userDetails.Username = System.Console.ReadLine();

            System.Console.WriteLine("Please provide a first name!");
            userDetails.FirstName = System.Console.ReadLine();

            System.Console.WriteLine("Please provide a last name!");
            userDetails.LastName = System.Console.ReadLine();

            await user.RegisterUser(userDetails);
        }

        private static void InitializeWithRetries(ClientConfiguration config, int initializeAttemptsBeforeFailing)
        {
            var attempt = 0;
            while (true)
            {
                try
                {
                    GrainClient.Initialize(config);
                    System.Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    System.Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing) throw;
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

            }
        }
    }
}