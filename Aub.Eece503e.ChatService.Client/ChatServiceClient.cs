using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        private async Task EnsureSuccessOrThrow(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string message = $"{responseMessage.ReasonPhrase}, {await responseMessage.Content.ReadAsStringAsync()}";
                throw new ChatServiceException(message, responseMessage.StatusCode);
            }
        }
        
        public async Task<UserProfile> GetProfile(string username)
        {
            var responseMessage = await _httpClient.GetAsync($"api/profile/{username}");
            await EnsureSuccessOrThrow(responseMessage);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedProfile = JsonConvert.DeserializeObject<UserProfile>(json);
            return fetchedProfile;
        }

        public async Task AddProfile(UserProfile userProfile)
        {
            var json = JsonConvert.SerializeObject(userProfile);
            var responseMessage = await _httpClient.PostAsync("api/profile",
                new StringContent(json, Encoding.UTF8, "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }

        public async Task UpdateProfile(UserProfile profile)
        {
            var json = JsonConvert.SerializeObject(profile);
            var responseMessage = await _httpClient.PutAsync($"api/profile/{profile.Username}",
                new StringContent(json, Encoding.UTF8, "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }

        public async Task DeleteProfile(string username)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/profile/{username}");
            await EnsureSuccessOrThrow(responseMessage);
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
                await EnsureSuccessOrThrow(responseMessage);
                var json = await responseMessage.Content.ReadAsStringAsync();
                var fetchedImageResponse = JsonConvert.DeserializeObject<UploadImageResponse>(json);
                return fetchedImageResponse;
            }
        }

        public async Task<DownloadImageResponse> DownloadImage(string imageId)
        {
            using (HttpResponseMessage responseMessage = await _httpClient.GetAsync($"api/images/{imageId}"))
            {
                await EnsureSuccessOrThrow(responseMessage);
                var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                return new DownloadImageResponse(bytes);
            }
        }

        public async Task DeleteImage(string imageId)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/images/{imageId}");
            await EnsureSuccessOrThrow(responseMessage);
        }

        public async Task AddMessage(string conversationId, AddMessageRequestBody addMessageRequestBody)
        {
            string json = JsonConvert.SerializeObject(addMessageRequestBody);
            HttpResponseMessage response = await _httpClient.PostAsync($"api/conversations/{conversationId}/messages", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrow(response);
        }

        public Task<GetMessagesResponse> GetMessages(string conversationId, int limit, long lastSeenMessageTime)
        {
            return GetMessagesByUri($"api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}");
        }

        public async Task<GetMessagesResponse> GetMessagesByUri(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            await EnsureSuccessOrThrow(response);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetMessagesResponse>(json);
        }
        
        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages/{messageId}");
            await EnsureSuccessOrThrow(response);
            string json = await response.Content.ReadAsStringAsync();
            var message =  JsonConvert.DeserializeObject<Message>(json);
            return message;
        }

        public async Task DeleteMessage(string conversationId, string messageId)
        {
            var response = await _httpClient.DeleteAsync($"api/conversations/{conversationId}/messages/{messageId}");
            await EnsureSuccessOrThrow(response);
        }

        public async Task AddConversation(AddConversationRequestBody addConversationRequestBody)
        {
            string json = JsonConvert.SerializeObject(addConversationRequestBody);
            HttpResponseMessage response = await _httpClient.PostAsync($"api/conversations", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrow(response);
        }

        public Task<GetConversationsResponse> GetConversations(string username, int limit, long lastSeenConversationTime)
        {
            return GetConversationsByUri($"api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}");
        }

        public async Task<GetConversationsResponse> GetConversationsByUri(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            await EnsureSuccessOrThrow(response);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetConversationsResponse>(json);
        }

        public async Task<Conversation> GetConversation(string conversationId)
        {
            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}");
            await EnsureSuccessOrThrow(response);
            string json = await response.Content.ReadAsStringAsync();
            var conversation =  JsonConvert.DeserializeObject<Conversation>(json);
            return conversation;
        }

        public async Task DeleteConversation(string username, string conversationId)
        {
            var response = await _httpClient.DeleteAsync($"api/conversations/{conversationId}/{username}");
            await EnsureSuccessOrThrow(response);
        }
    }
}