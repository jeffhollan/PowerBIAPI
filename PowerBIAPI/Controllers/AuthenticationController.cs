using Microsoft.Azure.AppService.ApiApps.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TRex.Metadata;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Web;

namespace PowerBIAPI.Controllers
{
    public class AuthenticationController : ApiController
    {
        private string clientId = ConfigurationManager.AppSettings["clientId"];
        private string redirectUrl = "https://" + ConfigurationManager.AppSettings["siteUrl"] + ".azurewebsites.net/redirect";
        private string clientSecret = ConfigurationManager.AppSettings["clientSecret"];

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("showRedirect")]
        public string ShowRedirect()
        {
            return "Your redirect URL is: " + redirectUrl;
        }

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("authorize")]
        public System.Web.Http.Results.RedirectResult Authorize()
        {
             
            return Redirect(String.Format("https://login.windows.net/common/oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}", clientId, HttpUtility.UrlEncode("https://analysis.windows.net/powerbi/api"), HttpUtility.UrlEncode(redirectUrl)));
        }

        [Metadata(Visibility = VisibilityType.Internal)]
        [HttpGet, Route("redirect")]
        public async Task<HttpResponseMessage> CompleteAuth(string code)
        {
            AuthenticationContext AC = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
            ClientCredential cc = new ClientCredential(clientId, clientSecret);
            AuthenticationResult ar = AC.AcquireTokenByAuthorizationCode(code, new Uri(redirectUrl), cc);
            PowerBIController.authorization = ar;
            await WriteTokenToStorage(ar);
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully Authenticated");
        }

        public async Task CheckToken()
        {
            if (PowerBIController.authorization == null)
                PowerBIController.authorization = await ReadTokenFromStorage();

            if (PowerBIController.authorization == null)
                return;

            if (DateTime.UtcNow.CompareTo(PowerBIController.authorization.ExpiresOn) >= 0)
            {
                AuthenticationContext AC = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
                ClientCredential cc = new ClientCredential(clientId, clientSecret);
                PowerBIController.authorization = await AC.AcquireTokenByRefreshTokenAsync(PowerBIController.authorization.RefreshToken, cc);
                await WriteTokenToStorage(PowerBIController.authorization);
            }
        }

        private async Task WriteTokenToStorage(AuthenticationResult ar)
        {
            var storage = Runtime.FromAppSettings().IsolatedStorage;
            await storage.WriteAsync("auth", JsonConvert.SerializeObject(ar));
        }

        private async Task<AuthenticationResult> ReadTokenFromStorage()
        {
            var storage = Runtime.FromAppSettings().IsolatedStorage;
            var authString = await storage.ReadAsStringAsync("auth");
            return JsonConvert.DeserializeObject<AuthenticationResult>(authString);
        }
    }
}
