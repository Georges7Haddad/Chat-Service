using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IProfileStore
    {
        Task<UserProfile> GetProfile(string username);
        Task AddProfile(UserProfile userProfile);
        Task UpdateProfile(UserProfile profile);
        Task DeleteProfile(string username);
    }
}