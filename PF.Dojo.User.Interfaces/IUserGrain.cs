using System;
using System.Threading.Tasks;
using Orleans;

namespace PF.Dojo.User.Interfaces
{
    [Serializable]
    public class UserDetails
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public interface IUserGrain : IGrainWithGuidKey
    {
        Task RegisterUser(UserDetails userDetails);
    }
}