using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OidcSampleApp.Models;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Text;

namespace OidcSampleApp.Controllers
{    public class HomeController : Controller
    {
        private IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            // Get the OneLogin user id for the current user
            var oneLoginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["Username"] = User.FindFirstValue(ClaimTypes.Name);
            ViewData["Region"] = _configuration["oidc:region"];

            if (!string.IsNullOrEmpty(_configuration["oidc:clientid"]) && !String.IsNullOrEmpty(_configuration["oidc:clientsecret"])){
                // Get a list of apps for this user
                var apps = await GetAppsForUser(oneLoginUserId);
                ViewData["Apps"] = apps;
            }

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private

        private async Task<List<UserApp>> GetAppsForUser(string userId){
            using(var client = new HttpClient()){

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", GetAccessToken());

                var res = await client.GetAsync(String.Format("https://{0}.onelogin.com/api/2/users/{1}/apps", _configuration["oidc:region"], userId));

                var json = await res.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<List<UserApp>>(json);

                return apiResponse;
            }
        }

        private string GetAccessToken(){
            using(var client = new HttpClient())
            {
                var credentials = string.Format("{0}:{1}", _configuration["clientid"], _configuration["clientsecret"]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))); 

                var body = JsonConvert.SerializeObject(new {
                    grant_type = "client_credentials"
                });

                var req = new HttpRequestMessage(){
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(String.Format("https://{0}.onelogin.com/auth/oauth2/v2/token", _configuration["oidc:region"])),
                    Content = new StringContent(body)
                };

                // We add the Content-Type Header like this because otherwise dotnet
                // adds the utf-8 charset extension to it which is not compatible with OneLogin
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var res = client.SendAsync(req).Result;

                var json = res.Content.ReadAsStringAsync().Result;

                var tokenReponse = JsonConvert.DeserializeObject<OAuthTokenResponse>(json);

                return tokenReponse.AccessToken;
            }
        }

        #endregion
    }
}
