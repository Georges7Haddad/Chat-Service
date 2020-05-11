using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public abstract class ProfileControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture
    {
        private readonly IChatServiceClient _chatServiceClient;

        private readonly ConcurrentBag<UserProfile> _profilesToCleanup = new ConcurrentBag<UserProfile>();

        public ProfileControllerEndToEndTests(TFixture tfixture)
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
            foreach (var profile in _profilesToCleanup)
                tasks.Add(_chatServiceClient.DeleteProfile(profile.Username));
            await Task.WhenAll(tasks);
        }

        private static string CreateRandomUsername()
        {
            return Guid.NewGuid().ToString();
        }

        [Fact]
        public async Task PostGetProfile()
        {
            var profilePictureId = CreateRandomUsername();
            var profile = new UserProfile { Username = CreateRandomUsername(), FirstName = "Georges", LastName = "Haddad", ProfilePictureId = profilePictureId };
            await _chatServiceClient.AddProfile(profile);
            _profilesToCleanup.Add(profile);

            var fetchedProfile = await _chatServiceClient.GetProfile(profile.Username);
            Assert.Equal(profile, fetchedProfile);
        }

        [Fact]
        public async Task UpdateProfile()
        {
            var username = CreateRandomUsername();
            var profile = new UserProfile { Username = username, FirstName = "Georges", LastName = "Haddad", ProfilePictureId = "georgesimage" };
            await _chatServiceClient.AddProfile(profile);
            _profilesToCleanup.Add(profile);

            var expectedProfile = new UserProfile { Username = username, FirstName = "Kamil", LastName = "Nader", ProfilePictureId = "kamilimage" };
            await _chatServiceClient.UpdateProfile(expectedProfile);

            var updatedProfile = await _chatServiceClient.GetProfile(username);
            Assert.Equal(expectedProfile, updatedProfile);
        }

        [Fact]
        public async Task DeleteProfile()
        {
            var username = CreateRandomUsername();
            var profile = new UserProfile { Username = username, FirstName = "Georges", LastName = "Haddad" };
            await _chatServiceClient.AddProfile(profile);

            await _chatServiceClient.DeleteProfile(profile.Username);

            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.GetProfile(profile.Username));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingProfile()
        {
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.GetProfile(CreateRandomUsername()));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingProfile()
        {
            var profile = new UserProfile { Username = CreateRandomUsername(), FirstName = "davie", LastName = "bass" };
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.UpdateProfile(profile));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task AddExistingProfile()
        {
            var username = CreateRandomUsername();
            var profile = new UserProfile { Username = username, FirstName = "Kerbi", LastName = "yes" };
            await _chatServiceClient.AddProfile(profile);
            _profilesToCleanup.Add(profile);

            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.AddProfile(profile));
            Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingProfile()
        {
            var exception =
                await Assert.ThrowsAsync<ChatServiceException>(() =>
                    _chatServiceClient.DeleteProfile(CreateRandomUsername()));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Theory]
        [InlineData("Ven", "", "kay")]
        [InlineData("tlp", "Pil", "")]
        [InlineData("", "Bert", "Berry")]
        [InlineData(null, "Beck", "Wood")]
        [InlineData("Key", null, "Bi")]
        [InlineData("three", "Pi", null)]
        [InlineData(" ", "Trev", "Mog")]
        [InlineData("Lip", " ", "Ty")]
        [InlineData("Yok", "Wem", " ")]
        public async Task PostInvalidProfile(string username, string firstName, string lastName)
        {
            var profile = new UserProfile { Username = username, FirstName = firstName, LastName = lastName };

            var exception = await Assert.ThrowsAsync<ChatServiceException>(() => _chatServiceClient.AddProfile(profile));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }

        [Theory]
        [InlineData("", "kay")]
        [InlineData("Pil", "")]
        [InlineData(null, "Wood")]
        [InlineData("Key", null)]
        [InlineData(" ", "Trev")]
        [InlineData("Lip", " ")]
        public async Task PutInvalidProfile(string firstname, string lastname)
        {
            var username = CreateRandomUsername();
            var profile = new UserProfile { Username = username, FirstName = "First", LastName = "Last" };
            await _chatServiceClient.AddProfile(profile);
            _profilesToCleanup.Add(profile);

            var updatedProfile = new UserProfile { Username = username, FirstName = firstname, LastName = lastname };
            var exception =
                await Assert.ThrowsAsync<ChatServiceException>(() => _chatServiceClient.UpdateProfile(updatedProfile));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }
    }
}
