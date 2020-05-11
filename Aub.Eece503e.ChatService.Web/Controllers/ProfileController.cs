using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            var profile = await _profileService.GetProfile(username);
            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserProfile userProfile)
        {
            await _profileService.AddProfile(userProfile);
            return CreatedAtAction(nameof(Get), new {username = userProfile.Username}, userProfile);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Put(string username, [FromBody] UpdateProfileRequestBody updateProfileRequestBody)
        {
            await _profileService.UpdateProfile(username, updateProfileRequestBody);
            return Ok();
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            await _profileService.DeleteProfile(username);
            return Ok();
        }
    }
}
