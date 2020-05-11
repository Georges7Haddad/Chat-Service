using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IImageService
    {
        Task<byte[]> DownloadImage(string imageId);
        Task<string> UploadImage(IFormFile file);
        Task DeleteImage(string imageId);
    }
}