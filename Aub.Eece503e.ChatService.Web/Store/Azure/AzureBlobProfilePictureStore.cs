using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureBlobProfilePictureStore:IImagesStore
    {
        private readonly CloudBlobContainer _profilePicturesContainer;
        
        public AzureBlobProfilePictureStore(IOptions<AzureStorageSettings> options)
        {
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ConnectionString"));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient(); 
            _profilePicturesContainer = cloudBlobClient.GetContainerReference("ProfilePictures"); 
            _profilePicturesContainer.CreateAsync();

            }
        public async Task<string> UploadImage(byte[] stream)
        {
            try
            {
                var imageid = Guid.NewGuid().ToString();
                CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageid);
                await cloudBlockBlob.UploadFromFileAsync(stream.ToString());
                return imageid;
            }
            catch (StorageException e)
            {
                throw new StorageException("Could not write to azure blob", e);
            }
        }

        public async Task<byte[]> DownloadImage(string imageId)
        {
            CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageId);
            long imageByteArrayLenght = cloudBlockBlob.Properties.Length;
            byte[] imageByteArray = new byte[imageByteArrayLenght];
            await cloudBlockBlob.DownloadToByteArrayAsync(imageByteArray,0 );
            return imageByteArray;
        }

        public async Task DeleteImage(string imageId)
        {
            CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageId);
            await cloudBlockBlob.DeleteAsync();
        }
    }
}