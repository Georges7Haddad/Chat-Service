using System.Diagnostics;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Exceptions;
using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileStore _profileStore;
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;
        
        public ProfileService(ILogger<ConversationsController> logger, TelemetryClient telemetryClient, IProfileStore profileStore)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _profileStore = profileStore;
        }

        public async Task<UserProfile> GetProfile(string username)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                var stopWatch = Stopwatch.StartNew();
                var profile = await _profileStore.GetProfile(username);
                _telemetryClient.TrackMetric("ProfileStore.GetProfile.Time", stopWatch.ElapsedMilliseconds);
                return profile;
            }
        }

        public async Task AddProfile(UserProfile profile)
        {
            using (_logger.BeginScope("{Username}", profile.Username))
            {
                if (!ValidateProfile(profile, out string error))
                    throw new BadRequestException(error);
                var stopWatch = Stopwatch.StartNew();
                await _profileStore.AddProfile(profile);
                
                _telemetryClient.TrackMetric("ProfileStore.AddProfile.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ProfileCreated");
            }
        }

        public async Task UpdateProfile(string username, UpdateProfileRequestBody updateProfileRequestBody)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                var userProfile = new UserProfile
                {
                    Username = username,
                    FirstName = updateProfileRequestBody.FirstName,
                    LastName = updateProfileRequestBody.LastName,
                    ProfilePictureId = updateProfileRequestBody.ProfilePictureId
                };

                if (!ValidateProfile(userProfile, out string error))
                    throw new BadRequestException(error);
                var stopWatch = Stopwatch.StartNew();
                await _profileStore.UpdateProfile(userProfile);
                
                _telemetryClient.TrackMetric("ProfileStore.UpdateProfile.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ProfileUpdated");
            }
        }

        public async Task DeleteProfile(string username)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                var stopWatch = Stopwatch.StartNew();
                await _profileStore.DeleteProfile(username);
                
                _telemetryClient.TrackMetric("ProfileStore.DeleteProfile.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ProfileDeleted");
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