using Aub.Eece503e.ChatService.Client;
using Microsoft.AspNetCore.TestHost;


namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public class IntegrationTestsFixture
    {
        public ChatServiceClient ChatServiceClient {get;}

        public IntegrationTestsFixture()
        {
            
            var testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            ChatServiceClient = new ChatServiceClient(httpClient); 
        }
    }
}