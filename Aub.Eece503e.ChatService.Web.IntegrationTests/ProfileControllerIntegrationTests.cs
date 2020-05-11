using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public class ProfileControllerIntegrationTests : ProfileControllerEndToEndTests<IntegrationTestsFixture>
    {
        public ProfileControllerIntegrationTests(IntegrationTestsFixture fixture) : base(fixture)
        {
        }
    }
}