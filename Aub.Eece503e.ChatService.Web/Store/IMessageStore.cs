using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IMessageStore
    {
        Task<Message> GetMessage(string conversationId, string messageId);
        Task AddMessage(string conversationId, Message message);
        Task DeleteMessage(string conversationId, string messageId);
        Task<GetMessagesResult> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime);
    }
}