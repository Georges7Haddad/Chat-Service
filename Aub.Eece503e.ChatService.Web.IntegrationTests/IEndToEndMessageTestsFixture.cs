using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.DocumentDb;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public interface IEndToEndMessageTestsFixture
    { 
        public IMessageStore MessageStore { get; }
    }
}