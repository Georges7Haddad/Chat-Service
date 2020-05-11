using System.IO;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Client
{
    public interface IChatServiceClient
    {
        Task<UserProfile> GetProfile(string username);
        Task AddProfile(UserProfile userProfile);
        Task UpdateProfile(UserProfile profile);
        Task DeleteProfile(string username);
        Task<UploadImageResponse> UploadImage(Stream stream);
        Task<DownloadImageResponse> DownloadImage(string imageId);
        Task DeleteImage(string imageId);
        Task AddMessage(string conversationId, AddMessageRequestBody addMessageRequestBody);
        Task<GetMessagesResponse> GetMessages(string conversationId, int limit, long lastSeenMessageTime);
        Task<GetMessagesResponse> GetMessagesByUri(string uri);
        Task<Message> GetMessage(string conversationId, string messageId);
        Task DeleteMessage(string conversationId, string messageId);
        Task AddConversation(AddConversationRequestBody addConversationRequestBody);
        Task<GetConversationsResponse> GetConversations(string username, int limit, long lastSeenConversationTime);
        Task<GetConversationsResponse> GetConversationsByUri(string uri);
        Task<Conversation> GetConversation(string conversationId);
        Task DeleteConversation(string username, string conversationId);
    }
}