using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public abstract class ImagesControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture
    {
            
        private readonly IChatServiceClient _chatServiceClient;

        private readonly ConcurrentBag<string> _profilePicturesToCleanup = new ConcurrentBag<string>();

        public ImagesControllerEndToEndTests(TFixture tfixture)
        {
            _chatServiceClient = tfixture.ChatServiceClient;
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

        private MemoryStream CreateRandomStream()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(bytes);
            return stream;
        }

        [Fact]
        public async Task UploadDownloadImage()
        {
            var stream = CreateRandomStream();
            var uploadImageResponse = await _chatServiceClient.UploadImage(stream);

            _profilePicturesToCleanup.Add(uploadImageResponse.ImageId);

            var fetchedProfilePicture = await _chatServiceClient.DownloadImage(uploadImageResponse.ImageId);
            var imageStream = new MemoryStream(fetchedProfilePicture.Image);
            Assert.Equal(stream.ToArray(), imageStream.ToArray());
        }

        [Fact]
        public async Task DeleteImage()
        {
            var stream = CreateRandomStream();
            var uploadImageResponse = await _chatServiceClient.UploadImage(stream);

            await _chatServiceClient.DeleteImage(uploadImageResponse.ImageId);

            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.DownloadImage(uploadImageResponse.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task DownloadNonExistingImage()
        {
            string str = Guid.NewGuid().ToString();
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.DownloadImage(str));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingImage()
        {
            string str = Guid.NewGuid().ToString();
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.DeleteImage(str));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }
    }
}
