using System;
using System.IO;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureBlobProfilePictureStore:IImagesStore
    {
        private readonly CloudBlobContainer _profilePicturesContainer;
        
        public AzureBlobProfilePictureStore(IOptions<AzureStorageSettings> options)
        {
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ConnectionString"));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient(); 
            _profilePicturesContainer = cloudBlobClient.GetContainerReference(options.Value.ProfilePictureContainerName);
        }
        public async Task<string> UploadImage(byte[] imageBytes)
        {
            try
            {
                var imageId = Guid.NewGuid().ToString();
                CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageId);
                var imageStream = new MemoryStream(imageBytes);
                await cloudBlockBlob.UploadFromStreamAsync(imageStream);
                return imageId;
            }
            catch (StorageException e)
            {
                throw new StorageException("Could not write to azure blob", e);
            }
        }

        public async Task<byte[]> DownloadImage(string imageId)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageId);
                
                if(!await cloudBlockBlob.ExistsAsync())
                    throw new ProfilePictureNotFoundException("Profile Picture not found");

                var imageByteLength = cloudBlockBlob.Properties.Length;
                var imageBytes = new byte[imageByteLength];
                await cloudBlockBlob.DownloadToByteArrayAsync(imageBytes, 0);
                return imageBytes;
            }
            catch (StorageException e)
            { 
                throw new StorageException("Could not read from azure blob", e);
            }
        }

        public async Task DeleteImage(string imageId)
        {
            try
            {
                CloudBlockBlob cloudBlockBlob = _profilePicturesContainer.GetBlockBlobReference(imageId);
                if(!await cloudBlockBlob.ExistsAsync())
                    throw new ProfilePictureNotFoundException("Profile Picture not found");
                await cloudBlockBlob.DeleteAsync();
            }
            catch (StorageException e)
            {
                throw new StorageException("Could not delete from blob", e);
            }
        }
    }
}