using System.Net;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class AssertUtils
    {
        public static void HasStatusCode(HttpStatusCode statusCode, IActionResult actionResult)
        {
            Assert.True(actionResult is ObjectResult);
            var objectResult = (ObjectResult) actionResult;

            Assert.Equal((int) statusCode, objectResult.StatusCode);
        }
    }
}