using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            byte[] bytes = await _imageService.DownloadImage(id);
            return new FileContentResult(bytes, "application/octet-stream");
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            string imageId = await _imageService.UploadImage(file);
            return CreatedAtAction(nameof(DownloadImage), new {Id = imageId},
            new UploadImageResponse
            {
                ImageId = imageId
            });
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            await _imageService.DeleteImage(id);
            return Ok();
        }
    }
    
}