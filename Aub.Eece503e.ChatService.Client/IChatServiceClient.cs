using System.IO;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Client
{
    public interface IChatServiceClient
    {
        Task<UserProfile> GetProfile(string username);
        Task AddProfile(UserProfile userProfile);
        Task UpdateProfile(UserProfile profile);
        Task DeleteProfile(string username);
        Task<UploadImageResponse> UploadImage(Stream stream);
        Task<DownloadImageResponse> DownloadImage(string imageId);
        Task DeleteImage(string imageId);
    }
}