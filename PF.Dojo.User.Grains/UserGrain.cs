using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using PF.Dojo.User.Interfaces;

namespace PF.Dojo.User.Grains
{
	public class UserGrainState
	{
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	[StorageProvider(ProviderName = "DDBStore")]
	public class UserGrain : Grain<UserGrainState>, IUserGrain
	{
		public Task RegisterUser(UserDetails userDetails)
		{
			State.FirstName = userDetails.FirstName;
			State.LastName = userDetails.LastName;
			State.Username = userDetails.Username;

			Console.WriteLine(
				$"Have been set to GrainId: '{this.GetPrimaryKey()}', username: '{userDetails.Username}', " +
				$"First Name: '{userDetails.FirstName}' & Last Name: '{userDetails.LastName}'");

			WriteStateAsync();
			return TaskDone.Done;
		}

		public Task<string> GetUsername()
		{
			return Task.FromResult(State.Username);
		}

		public Task<string> GetFirstName()
		{
			return Task.FromResult(State.FirstName);
		}

		public Task<string> GetLastName()
		{
			return Task.FromResult(State.LastName);
		}

		public override Task OnActivateAsync()
		{
			var grainId = this.GetPrimaryKeyString();
			Console.WriteLine($"Activated Grain {grainId}");

			return TaskDone.Done;
		}
	}
}