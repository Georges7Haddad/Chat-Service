using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Newtonsoft.Json;

namespace Aub.Eece503e.ChatService.Client
{
    public class ChatServiceClient : IChatServiceClient
    {
        private readonly HttpClient _httpClient;

        public ChatServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static void EnsureSuccessOrThrow(HttpResponseMessage httpResponseMessage, string message)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new ChatServiceException(message, httpResponseMessage.StatusCode);
        }

        public async Task<UserProfile> GetProfile(string username)
        {
            var responseMessage = await _httpClient.GetAsync($"api/profile/{username}");
            EnsureSuccessOrThrow(responseMessage, "Get UserProfile Exception");
            var json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedProfile = JsonConvert.DeserializeObject<UserProfile>(json);
            return fetchedProfile;
        }

        public async Task AddProfile(UserProfile userProfile)
        {
            var json = JsonConvert.SerializeObject(userProfile);
            var responseMessage = await _httpClient.PostAsync("api/profile",
                new StringContent(json, Encoding.UTF8, "application/json"));
            EnsureSuccessOrThrow(responseMessage, "Add UserProfile Exception");
        }

        public async Task UpdateProfile(UserProfile profile)
        {
            var json = JsonConvert.SerializeObject(profile);
            var responseMessage = await _httpClient.PutAsync($"api/profile/{profile.Username}",
                new StringContent(json, Encoding.UTF8, "application/json"));
            EnsureSuccessOrThrow(responseMessage, "Update UserProfile Exception");
        }

        public async Task DeleteProfile(string username)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/profile/{username}");
            EnsureSuccessOrThrow(responseMessage, "Delete UserProfile Exception");
        }

        public async Task<UploadImageResponse> UploadImage(Stream stream)
        {
            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "NotNeeded"
            };
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(fileStreamContent);
                var request = new HttpRequestMessage(HttpMethod.Post, "api/images")
                {
                    Content = formData
                };
                var responseMessage = await _httpClient.SendAsync(request);
                EnsureSuccessOrThrow(responseMessage, "Upload Image Exception");
                var json = await responseMessage.Content.ReadAsStringAsync();
                var fetchedImageResponse = JsonConvert.DeserializeObject<UploadImageResponse>(json);
                return fetchedImageResponse;
            }
        }

        public async Task<DownloadImageResponse> DownloadImage(string imageId)
        {
            using (HttpResponseMessage responseMessage = await _httpClient.GetAsync($"api/images/{imageId}"))
            {
                EnsureSuccessOrThrow(responseMessage, "Download Image Exception");
                var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                return new DownloadImageResponse(bytes);
            }
        }

        public async Task DeleteImage(string imageId)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/images/{imageId}");
            EnsureSuccessOrThrow(responseMessage, "Delete Image Exception");
        }
    }
}