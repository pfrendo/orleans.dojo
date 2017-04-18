using System;
using System.Threading.Tasks;
using Orleans;
using PF.Dojo.User.Interfaces;

namespace PF.Dojo.User.Grains
{
	public class UserGrain : Grain, IUserGrain
	{
		private string _username;

		public override Task OnActivateAsync()
		{
			var grainId = this.GetPrimaryKeyString();
			Console.WriteLine($"Activated Grain {grainId}");

			if (!string.IsNullOrWhiteSpace(_username))
				Console.WriteLine($"Username was already set by '{grainId}'.");

			return TaskDone.Done;
		}

		public Task SetUsername(string username)
		{
			_username = username;
			Console.WriteLine($"Username has been set to: '{username}'");
			return TaskDone.Done;
		}
	}
}
