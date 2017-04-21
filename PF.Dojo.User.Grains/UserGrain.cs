using System;
using System.Threading.Tasks;
using Orleans;
using PF.Dojo.User.Interfaces;

namespace PF.Dojo.User.Grains
{
	public class UserGrain : Grain, IUserGrain
	{
		private UserDetails _userDetails;

	    public override Task OnActivateAsync()
		{
			var grainId = this.GetPrimaryKeyString();
			Console.WriteLine($"Activated Grain {grainId}");

			if (_userDetails != null)
				Console.WriteLine($"User Details have been set by '{grainId}'.");

			return TaskDone.Done;
		}

	    public Task RegisterUser(UserDetails userDetails)
	    {
	        _userDetails = userDetails;

	        Console.WriteLine($"Have been set to GrainId: '{this.GetPrimaryKey()}', username: '{_userDetails.Username}', " +
	                          $"First Name: '{_userDetails.FirstName}' & Last Name: '{_userDetails.LastName}'");
            
	        return TaskDone.Done;
        }
	}
}
