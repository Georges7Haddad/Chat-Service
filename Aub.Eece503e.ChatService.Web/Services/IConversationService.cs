using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IConversationService
    {
        Task<AddConversationResponse> AddConversation(AddConversationRequestBody addConversationRequestBody);
        Task<GetConversationsResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
        Task<Conversation> GetConversation(string conversationId);
        Task DeleteConversation(string username, string conversationId);
    }
}