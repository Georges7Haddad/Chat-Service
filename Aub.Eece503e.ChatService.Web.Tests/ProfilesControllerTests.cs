using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class ProfilesControllerTests
    {
        private readonly Mock<IProfileStore> _profilesStoreMock = new Mock<IProfileStore>();

        private readonly UserProfile _testUserProfile = new UserProfile
        {
            Username = "trev",
            FirstName = "Georges",
            LastName = "Haddad"
        };

        private readonly UpdateProfileRequestBody _updateProfileRequest = new UpdateProfileRequestBody
        {
            FirstName = "Kamil",
            LastName = "Ryan"
        };

        [Fact]
        public async Task GetProfileReturns503WhenStorageIsDown()
        {
            _profilesStoreMock.Setup(store => store.GetProfile(_testUserProfile.Username))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ProfileController(Mock.Of<ILogger<ProfileController>>(), _profilesStoreMock.Object);

            var result = await controller.Get(_testUserProfile.Username);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task GetProfileReturns500WhenExceptionIsNotKnown()
        {
            _profilesStoreMock.Setup(store => store.GetProfile(_testUserProfile.Username))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();

            var controller = new ProfileController(loggerStub, _profilesStoreMock.Object);

            var result = await controller.Get(_testUserProfile.Username);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task AddProfileReturns503WhenStorageIsDown()
        {
            _profilesStoreMock.Setup(store => store.AddProfile(_testUserProfile))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ProfileController(Mock.Of<ILogger<ProfileController>>(), _profilesStoreMock.Object);

            var result = await controller.Post(_testUserProfile);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task AddProfileReturns500WhenExceptionIsNotKnown()
        {
            _profilesStoreMock.Setup(store => store.AddProfile(_testUserProfile))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();

            var controller = new ProfileController(loggerStub, _profilesStoreMock.Object);

            var result = await controller.Post(_testUserProfile);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task PutProfileReturns503WhenStorageIsDown()
        {
            var profile = new UserProfile
            {
                Username = _testUserProfile.Username,
                FirstName = _updateProfileRequest.FirstName,
                LastName = _updateProfileRequest.LastName
            };
            _profilesStoreMock.Setup(store => store.UpdateProfile(profile))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ProfileController(Mock.Of<ILogger<ProfileController>>(), _profilesStoreMock.Object);

            var result = await controller.Put(_testUserProfile.Username, _updateProfileRequest);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task PutProfileReturns500WhenExceptionIsNotKnown()
        {
            var profile = new UserProfile
            {
                Username = _testUserProfile.Username,
                FirstName = _updateProfileRequest.FirstName,
                LastName = _updateProfileRequest.LastName
            };

            _profilesStoreMock.Setup(store => store.UpdateProfile(profile))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();

            var controller = new ProfileController(loggerStub, _profilesStoreMock.Object);

            var result = await controller.Put(_testUserProfile.Username, _updateProfileRequest);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DeleteProfileReturns503WhenStorageIsDown()
        {
            _profilesStoreMock.Setup(store => store.DeleteProfile(_testUserProfile.Username))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ProfileController(Mock.Of<ILogger<ProfileController>>(), _profilesStoreMock.Object);

            var result = await controller.Delete(_testUserProfile.Username);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }


        [Fact]
        public async Task DeleteProfileReturns500WhenExceptionIsNotKnown()
        {
            _profilesStoreMock.Setup(store => store.DeleteProfile(_testUserProfile.Username))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();

            var controller = new ProfileController(loggerStub, _profilesStoreMock.Object);

            var result = await controller.Delete(_testUserProfile.Username);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }
    }
}