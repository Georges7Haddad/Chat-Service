using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IProfileService
    {
        Task<UserProfile> GetProfile(string username);
        Task AddProfile(UserProfile profile);
        Task UpdateProfile(string username, UpdateProfileRequestBody updateProfileRequestBody);
        Task DeleteProfile(string username);
    }
}