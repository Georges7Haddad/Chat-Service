using System;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileStore _users;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ILogger<ProfileController> logger, IProfileStore users)
        {
            _logger = logger;
            _users = users;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            try
            {
                var profile = await _users.GetProfile(username);
                return Ok(profile);
            }
            catch (ProfileNotFoundException)
            {
                return NotFound($"UserProfile with username {username} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to retrieve userProfile {username} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while retrieving userProfile {username} from storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserProfile userProfile)
        {
            try
            {
                if (!ValidateProfile(userProfile, out string error))
                    return BadRequest(error);
                
                await _users.AddProfile(userProfile);
                return CreatedAtAction(nameof(Get), new { username = userProfile.Username}, userProfile);
            }
            catch (ProfileAlreadyExistsException)
            {
                return Conflict($"Username {userProfile.Username} is not available");
            }
            
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to add userProfile {userProfile} to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding userProfile {userProfile.Username} to storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Put(string username, [FromBody] UpdateProfileRequestBody updateProfileRequestBody)
            
        {
            
            try
            {
                var userProfile = new UserProfile
                {
                    Username = username,
                    FirstName = updateProfileRequestBody.FirstName,
                    LastName = updateProfileRequestBody.LastName
                };
                
                if (!ValidateProfile(userProfile, out string error))
                    return BadRequest(error);
                
                await _users.UpdateProfile(userProfile);
                return Ok();
            }
            catch (ProfileNotFoundException)
            {
                return NotFound($"UserProfile with username {username} was not found");
            }
            
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to update userProfile {updateProfileRequestBody} in storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while updating userProfile {updateProfileRequestBody} in storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
            
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            try
            {
                await _users.DeleteProfile(username);
                return Ok();
            }
            catch (ProfileNotFoundException)
            {
                return NotFound($"UserProfile with username {username} was not found");
            }
            
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to delete userProfile with username {username} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while deleting userProfile {username} from storage");
                return StatusCode(500, "An internal server error occured, please contact us if this error persists");
            }
        }
        
        private bool ValidateProfile(UserProfile userProfile, out string error)
        {
            if (string.IsNullOrWhiteSpace(userProfile.Username))
            {
                error = "The Username must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(userProfile.FirstName))
            {
                error = "The First Name must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(userProfile.LastName))
            {
                error = "The Last Name must not be empty";
                return false;
            }
            error = "";
            return true;
        }
    }
}
