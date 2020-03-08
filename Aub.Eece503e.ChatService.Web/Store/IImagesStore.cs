using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IImagesStore
    {
        Task <string> UploadImage(byte[] stream);
        Task <byte[]> DownloadImage(string imageId);
        Task DeleteImage(string imageId);
    }
    
}