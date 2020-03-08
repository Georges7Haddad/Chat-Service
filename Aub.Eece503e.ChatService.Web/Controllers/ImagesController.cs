using System;
using System.IO;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImagesStore _imagesStore;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(ILogger<ImagesController> logger, IImagesStore imagesStore)
        {
            _logger = logger;
            _imagesStore = imagesStore;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            try
            {
                byte[] bytes = await _imagesStore.DownloadImage(id);
                return new FileContentResult(bytes, "application/octet-stream");
            }
            catch (ProfileNotFoundException e)
            {
                return NotFound($"Image with id {id} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to get Image {id} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while retrieving Image {id} from storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
            
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    string imageId = await _imagesStore.UploadImage(stream.ToArray());
                    return CreatedAtAction(nameof(DownloadImage), new {Id = imageId},
                        new UploadImageResponse
                        {
                            ImageId = imageId
                        });
                }
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to add Image to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding Image to storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            try
            {
                await _imagesStore.DownloadImage(id);
                return Ok();
            }
            catch (ProfileNotFoundException e)
            {
                return NotFound($"Image with id {id} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to get Image {id} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while retrieving Image {id} from storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
        }
    }
    
}