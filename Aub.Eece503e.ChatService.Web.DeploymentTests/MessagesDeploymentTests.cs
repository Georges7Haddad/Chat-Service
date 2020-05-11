using Aub.Eece503e.ChatService.Web.IntegrationTests;

namespace Aub.Eece503e.ChatService.Web.DeploymentTests
{
    public class MessagesDeploymentTests : MessageControllerEndToEndTests<DeploymentTestsFixture>
    {
        public MessagesDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}