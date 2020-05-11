using Aub.Eece503e.ChatService.Web.IntegrationTests;

namespace Aub.Eece503e.ChatService.Web.DeploymentTests
{
    public class ImagesControllerDeploymentTests : ImagesControllerEndToEndTests<DeploymentTestsFixture>
    {
        public ImagesControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}