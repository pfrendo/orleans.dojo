using System.Threading.Tasks;
using Orleans;

namespace PF.Dojo.User.Interfaces
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task SetUsername(string username);
    }
}
