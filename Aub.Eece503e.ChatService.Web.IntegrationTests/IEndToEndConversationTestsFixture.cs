using Aub.Eece503e.ChatService.Web.Store;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public interface IEndToEndConversationTestsFixture
    {
        public IConversationStore ConversationStore { get; }
    }
}