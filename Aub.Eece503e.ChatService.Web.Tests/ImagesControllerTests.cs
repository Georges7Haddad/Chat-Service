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
    public class ImagesControllerTests
    {
        private readonly Mock<IImagesStore> _profilePicturesStoreMock = new Mock<IImagesStore>();

        private readonly string imageId = "Test";
        private readonly byte[] imageBytes = new byte[100];
        
        [Fact]
        public async Task DownloadProfilePictureReturns503WhenStorageIsDown()
        {
            _profilePicturesStoreMock.Setup(store => store.DownloadImage(imageId))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);

            var result = await controller.DownloadImage(imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task DownloadProfilePictureReturns500WhenExceptionIsNotKnown()
        {
            _profilePicturesStoreMock.Setup(store => store.DownloadImage(imageId))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ImagesControllerLoggerStub();

            var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);

            var result = await controller.DownloadImage(imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        // [Fact]
        // public async Task UploadProfilePictureReturns503WhenStorageIsDown()
        // {
        //     _profilePicturesStoreMock.Setup(store => store.UploadImage(imageBytes))
        //         .ThrowsAsync(new StorageErrorException());
        //
        //     var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);
        //
        //     var result = await controller.UploadImage(imageBytes);
        //     AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        // }
        //
        // [Fact]
        // public async Task UploadProfilePictureReturns500WhenExceptionIsNotKnown()
        // {
        //     _profilePicturesStoreMock.Setup(store => store.UploadImage(imageBytes))
        //         .ThrowsAsync(new Exception("Test Exception"));
        //     var loggerStub = new ImagesControllerLoggerStub();
        //
        //     var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);
        //
        //     var result = await controller.UploadImage(imageBytes);
        //     AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
        //
        //     Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        // }
        
        [Fact]
        public async Task DeleteProfilePictureReturns503WhenStorageIsDown()
        {
            _profilePicturesStoreMock.Setup(store => store.DeleteImage(imageId))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);

            var result = await controller.DeleteImage(imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task DeleteProfilePictureReturns500WhenExceptionIsNotKnown()
        {
            _profilePicturesStoreMock.Setup(store => store.DeleteImage(imageId))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ImagesControllerLoggerStub();

            var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);

            var result = await controller.DeleteImage(imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }
    }
}