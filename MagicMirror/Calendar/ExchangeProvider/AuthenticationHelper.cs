//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Graph;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Microsoft.Identity.Client;

namespace MagicMirror.Calendar.ExchangeProvider
{
    internal static class AuthenticationHelper
    {
        // The Client ID is used by the application to uniquely identify itself to Microsoft Azure Active Directory (AD).
        static string clientId = App.Current.Resources["ida:ClientID"].ToString();
        static string returnUrl = App.Current.Resources["ida:ReturnUrl"].ToString();
        static string adTenantId = App.Current.Resources["ida:AdTenantId"].ToString();


        //public static PublicClientApplication IdentityClientApp = null;
        public static ConfidentialClientApplication _clientApp = null;
        public static string TokenForUser = null;
        public static DateTimeOffset expiration;

        private static GraphServiceClient graphClient = null;

        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static GraphServiceClient GetAuthenticatedClient()
        {
            if (graphClient == null)
            {
                // Create Microsoft Graph client.
                try
                {
                    graphClient = new GraphServiceClient(
                        "https://graph.microsoft.com/v1.0",
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var token = await GetTokenForUserAsync();
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                // This header has been added to identify our sample in the Microsoft Graph service.  If extracting this code for your project please remove.
                                //requestMessage.Headers.Add("SampleID", "uwp-csharp-snippets-sample");
                                requestMessage.Headers.Add("Prefer", "outlook.timezone=\"Eastern Standard Time\"");

                            }));
                    return graphClient;
                }

                catch (Exception ex)
                {
                    throw new Exception("Could not create a graph client: " + ex.Message);
                }
            }

            return graphClient;
        }


        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            if (TokenForUser == null || expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                var redirectUri = new Uri(returnUrl);
                var scopes = new string[]
                    {
                        "https://graph.microsoft.com/.default"
                    };

                if (_clientApp == null)
                {
                    var authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/oauth2/v2.0", adTenantId);
                    _clientApp = new ConfidentialClientApplication(authority, clientId, returnUrl, new ClientCredential("2adNeH9aywjbNyCGYDUbzyv"), null);
                }

                AuthenticationResult authResult;
                try
                {
                    authResult = await _clientApp.AcquireTokenSilentAsync(scopes);
                }
                catch
                {
                    try
                    {
                        authResult = await _clientApp.AcquireTokenForClient(scopes, null);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not acquire authentication token: " + ex.Message);
                    }
                }

                TokenForUser = authResult.Token;
                expiration = authResult.ExpiresOn;
            }

            return TokenForUser;
        }


        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            foreach (var user in _clientApp.Users)
            {
                user.SignOut();
            }
            graphClient = null;
            TokenForUser = null;

        }


    }
}
