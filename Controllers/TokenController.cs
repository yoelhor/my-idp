using System;
using Microsoft.AspNetCore.Mvc;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;

namespace my_idp.oauth2.Controllers
{
    public class TokenController : Controller
    {
        private static Lazy<X509SigningCredentials> SigningCredentials = null!;
        private readonly ILogger<TokenController> _logger;

        public TokenController(ILogger<TokenController> logger)
        {
            this._logger = logger;
            SigningCredentials = Commons.LoadCertificate();
        }

        [HttpGet]
        [ActionName("index")]
        public async Task<IActionResult> IndexAsyncGet(string code)
        {
            return await IndexCommonAsync(code);
        }

        [HttpPost]
        [ActionName("index")]
        public async Task<IActionResult> IndexAsyncPost(string code)
        {
            return await IndexCommonAsync(code);
        }

        private async Task<IActionResult> IndexCommonAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return new BadRequestObjectResult(new { error = "Code parameter is missing." });
            }

            // Try to get teh client_id and client_secret
            string ClientId = string.Empty;
            string ClientSecret = string.Empty;

            if (Request.Method == "POST")
            {
                ClientId = this.Request.Form["client_id"];
                ClientSecret = this.Request.Form["client_secret"];
            }
            else
            {
                ClientId = this.Request.Query["client_id"];
                ClientSecret = this.Request.Query["client_secret"];
            }

            // Check if client_secret_post authentication method is used
            if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret))
            {
                // Check credentials
                Oauth2TenantConfig settings = new Oauth2TenantConfig();
                if (settings.Token.CheckCredentials &&
                    (settings.ClientId != ClientId || settings.ClientSecret != ClientSecret))
                {
                    return new UnauthorizedObjectResult(new { error = "Invalid client_secret_post credentials" });
                }
            }

            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(code);
                string codeString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                HomeViewModel model = new HomeViewModel();

                string client_id = codeString.Split('|')[1];
                string email = codeString.Split('|')[2];
                string JWT = Commons.BuildJwtToken(TokenController.SigningCredentials.Value, this.Request, model);

                DateTime not_before = DateTime.Now.AddSeconds(-30);
                long not_beforeUnixTime = ((DateTimeOffset)not_before).ToUnixTimeSeconds();

                DateTime expires_on = DateTime.Now.AddDays(7);
                long expires_onUnixTime = ((DateTimeOffset)expires_on).ToUnixTimeSeconds();

                var payload = new
                {
                    access_token = JWT,
                    id_token = JWT,
                    token_type = "bearer",
                    refresh_token = "2723ff54-a7c6-4f66-b34a-332fcb9980b8",
                    not_before = not_beforeUnixTime,
                    expires_in = 43199,
                    expires_on = expires_onUnixTime,
                    scope = "email openid"
                };

                return new OkObjectResult(payload);
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult(ex.Message + " Code: " + code);
            }
        }
    }
}