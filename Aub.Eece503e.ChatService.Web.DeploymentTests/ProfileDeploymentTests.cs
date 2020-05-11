
using Aub.Eece503e.ChatService.Web.IntegrationTests;

namespace Aub.Eece503e.ChatService.Web.DeploymentTests
{
    public class ProfileControllerDeploymentTests : ProfileControllerEndToEndTests<DeploymentTestsFixture>
    {
        public ProfileControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}
