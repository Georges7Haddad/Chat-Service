using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class ImagesControllerTests
    {
        private static Mock<IImagesStore> imageStoreMock = new Mock<IImagesStore>();

        static TestServer testServer = new TestServer(
            Program.CreateWebHostBuilder(new string[] { })
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(imageStoreMock.Object);
                }).UseEnvironment("Development"));

        static HttpClient httpClient = testServer.CreateClient();
        static ChatServiceClient chatServiceClient = new ChatServiceClient(httpClient);
        
        private readonly string _imageId = "Test";
        private readonly byte[] _imageBytes = Encoding.UTF8.GetBytes("test");
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ImageControllerUploadImageStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            imageStoreMock.Setup(store => store.UploadImage(_imageBytes))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            var stream = new MemoryStream(_imageBytes);
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.UploadImage(stream));
            Assert.Equal(statusCode, e.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ImageControllerDownloadImageStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            imageStoreMock.Setup(store => store.DownloadImage(_imageId))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int) statusCode));
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.DownloadImage(_imageId));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ImageControllerDeleteImageStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            imageStoreMock.Setup(store => store.DeleteImage(_imageId))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.DeleteImage(_imageId));
            Assert.Equal(statusCode, e.StatusCode);
        }
    }
}