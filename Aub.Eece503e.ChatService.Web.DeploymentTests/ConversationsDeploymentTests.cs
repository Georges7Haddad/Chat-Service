using Aub.Eece503e.ChatService.Web.IntegrationTests;

namespace Aub.Eece503e.ChatService.Web.DeploymentTests
{
    public class ConversationsDeploymentTests : ConversationControllerEndtoEndTests<DeploymentTestsFixture>
    {
        public ConversationsDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}