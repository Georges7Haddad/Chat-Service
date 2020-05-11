using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IImagesStore _imagesStore;
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;
        
        public ImageService(ILogger<ConversationsController> logger, TelemetryClient telemetryClient, IImagesStore imagesStore)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _imagesStore = imagesStore;
        }

        public async Task<byte[]> DownloadImage(string imageId)
        {
            using (_logger.BeginScope("{ImageId}", imageId))
            {
                var stopWatch = Stopwatch.StartNew();
                byte[] bytes = await _imagesStore.DownloadImage(imageId);
                _telemetryClient.TrackMetric("ImageStore.DownloadImage.Time", stopWatch.ElapsedMilliseconds);
                return bytes;
            }
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                var stopWatch = Stopwatch.StartNew();
                string imageId = await _imagesStore.UploadImage(stream.ToArray());
                
                _telemetryClient.TrackMetric("ImageStore.UploadImage.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ImageUploaded");
                return imageId;
            }
        }

        public async Task DeleteImage(string imageId)
        {
            using (_logger.BeginScope("{ImageId}", imageId))
            {
                var stopWatch = Stopwatch.StartNew();
                await _imagesStore.DeleteImage(imageId);
                
                _telemetryClient.TrackMetric("ImageStore.DeleteImage.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ImageDeleted");
            }
        }
    }
}