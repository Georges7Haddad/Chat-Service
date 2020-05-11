using Aub.Eece503e.ChatService.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public interface IEndToEndTestsFixture
    {
        public IChatServiceClient ChatServiceClient { get; }
    }
}
