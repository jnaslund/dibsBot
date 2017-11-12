using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Graph;

namespace DibsBot.Helpers
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient = null;

        public static GraphServiceClient GetAuthenticadedClient(string token, string outlookTimezone = "America/Chicago")
        {
            graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                       async (requestMessage) =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                            requestMessage.Headers.Add("Prefer", $@"outlook.timezone=""{outlookTimezone}""");
                        }));
            return graphClient;
        }

        public static void SignOutClient()
        {
            graphClient = null;
        }
    }
}