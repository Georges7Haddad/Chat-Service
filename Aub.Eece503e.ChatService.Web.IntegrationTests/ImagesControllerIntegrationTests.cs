using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Xunit;
namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public class ImagesControllerIntegrationTests:IClassFixture<IntegrationTestsFixture>, IAsyncLifetime
    {
        private readonly IChatServiceClient _chatServiceClient;

        private readonly ConcurrentBag<string> _profilePicturesToCleanup = new ConcurrentBag<string>();

        public ImagesControllerIntegrationTests(IntegrationTestsFixture integrationTestsFixture)
        {
            _chatServiceClient = integrationTestsFixture.ChatServiceClient;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach (var profilePicture in _profilePicturesToCleanup)
                tasks.Add(_chatServiceClient.DeleteImage(profilePicture));
            await Task.WhenAll(tasks);
        }
        
        [Fact]
        public async Task UploadDownloadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(bytes);
            var uploadImageResponse = await _chatServiceClient.UploadImage(stream);
            // _profilePicturesToCleanup.Add(uploadImageResponse.ImageId);
            var fetchedProfilePicture = await _chatServiceClient.DownloadImage(uploadImageResponse.ImageId);
            var imageString = Encoding.UTF8.GetString(fetchedProfilePicture.Image);
            Assert.Equal(str,imageString);
        }
    }
}