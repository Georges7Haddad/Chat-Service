using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class ImagesControllerTests
    {
        private readonly Mock<IImagesStore> _profilePicturesStoreMock = new Mock<IImagesStore>();

        private readonly string _imageId = "Test";
        private readonly byte[] _imageBytes = Encoding.UTF8.GetBytes("test");

        
        [Fact]
        public async Task DownloadProfilePictureReturns503WhenStorageIsDown()
        {
            _profilePicturesStoreMock.Setup(store => store.DownloadImage(_imageId))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);

            var result = await controller.DownloadImage(_imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task DownloadProfilePictureReturns500WhenExceptionIsNotKnown()
        {
            _profilePicturesStoreMock.Setup(store => store.DownloadImage(_imageId))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ImagesControllerLoggerStub();

            var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);

            var result = await controller.DownloadImage(_imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task UploadProfilePictureReturns503WhenStorageIsDown()
        {
            var stream = new MemoryStream(_imageBytes);
            IFormFile file = new FormFile(stream, 0, _imageBytes.Length,"file", "image");
            
            
            _profilePicturesStoreMock.Setup(store => store.UploadImage(_imageBytes))
                .ThrowsAsync(new StorageErrorException());
        
            var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);
            
            var result = await controller.UploadImage(file);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }
        
        [Fact]
        public async Task UploadProfilePictureReturns500WhenExceptionIsNotKnown()
        {
            var stream = new MemoryStream(_imageBytes);
            IFormFile file = new FormFile(stream, 0, _imageBytes.Length,"file", "image");
            
            _profilePicturesStoreMock.Setup(store => store.UploadImage(_imageBytes))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ImagesControllerLoggerStub();
        
            var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);

            var result = await controller.UploadImage(file);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
        
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }
        
        [Fact]
        public async Task DeleteProfilePictureReturns503WhenStorageIsDown()
        {
            _profilePicturesStoreMock.Setup(store => store.DeleteImage(_imageId))
                .ThrowsAsync(new StorageErrorException());

            var controller = new ImagesController(Mock.Of<ILogger<ImagesController>>(), _profilePicturesStoreMock.Object);

            var result = await controller.DeleteImage(_imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [Fact]
        public async Task DeleteProfilePictureReturns500WhenExceptionIsNotKnown()
        {
            _profilePicturesStoreMock.Setup(store => store.DeleteImage(_imageId))
                .ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ImagesControllerLoggerStub();

            var controller = new ImagesController(loggerStub, _profilePicturesStoreMock.Object);

            var result = await controller.DeleteImage(_imageId);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }
    }
}