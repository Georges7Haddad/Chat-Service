using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IConversationStore
    {
        Task<Conversation> GetConversation(string username, string conversationId);
        Task AddConversation(Conversation conversation);
        Task DeleteConversation(string username, string conversationId);
        Task UpdateConversation(long unixTime, string conversationId);
        Task<GetConversationsResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
    }
}