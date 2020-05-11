using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Xunit;
namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public class ImagesControllerIntegrationTests : ImagesControllerEndToEndTests<IntegrationTestsFixture>
    {
        public ImagesControllerIntegrationTests(IntegrationTestsFixture fixture) : base(fixture)
        {

        }
    }
}