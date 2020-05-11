using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class ProfilesControllerTests
    {
        private static Mock<IProfileStore> profileStoreMock = new Mock<IProfileStore>();

        static TestServer testServer = new TestServer(
            Program.CreateWebHostBuilder(new string[] { })
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(profileStoreMock.Object);
                }).UseEnvironment("Development"));

        static HttpClient httpClient = testServer.CreateClient();
        static ChatServiceClient chatServiceClient = new ChatServiceClient(httpClient);
        
        private readonly UserProfile _testUserProfile = new UserProfile
        {
            Username = "trev",
            FirstName = "Georges",
            LastName = "Haddad"
        };
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ProfileControllerGetProfileStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            profileStoreMock.Setup(store => store.GetProfile(_testUserProfile.Username))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.GetProfile(_testUserProfile.Username));
            Assert.Equal(statusCode, e.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ProfileControllerAddProfileStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            profileStoreMock.Setup(store => store.AddProfile(_testUserProfile))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int) statusCode));
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.AddProfile(_testUserProfile));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ProfileControllerUpdateProfileStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            profileStoreMock.Setup(store => store.UpdateProfile(_testUserProfile))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.UpdateProfile(_testUserProfile));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ProfileControllerDeleteProfileStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            profileStoreMock.Setup(store => store.DeleteProfile(_testUserProfile.Username))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            ChatServiceException  e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.DeleteProfile(_testUserProfile.Username));
            Assert.Equal(statusCode, e.StatusCode);
        }
    }
}