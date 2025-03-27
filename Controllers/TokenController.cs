using System;
using Microsoft.AspNetCore.Mvc;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace my_idp.oauth2.Controllers
{
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        public TokenController(ILogger<TokenController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._configuration = configuration;
        }

        [HttpGet]
        [ActionName("index")]
        public IActionResult IndexAsyncGet(string code)
        {
            _logger.LogInformation($"#### HTTP GET call to /toekn");
            return IndexCommonAsync(code);
        }

        [HttpPost]
        [ActionName("index")]
        public IActionResult IndexAsyncPost(string code)
        {
            _logger.LogInformation($"#### HTTP POST call to /toekn");
            return IndexCommonAsync(code);
        }

        private IActionResult IndexCommonAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError($"#### Code parameter is missing");
                return new BadRequestObjectResult(new { error = "Code parameter is missing." });
            }

            // Try to get teh client_id and client_secret
            string ClientId = string.Empty;
            string ClientSecret = string.Empty;

            if (Request.Method == "POST")
            {
                ClientId = this.Request.Form["client_id"].ToString() ?? string.Empty;
                ClientSecret = this.Request.Form["client_secret"].ToString() ?? string.Empty;
            }
            else
            {
                ClientId = this.Request.Query["client_id"].ToString() ?? string.Empty;
                ClientSecret = this.Request.Query["client_secret"].ToString() ?? string.Empty;
            }

            // Check if client_secret_post authentication method is used
            if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret))
            {
                // Check credentials
                Oauth2TenantConfig settings = new Oauth2TenantConfig();
                if (_configuration.GetSection("AppSettings:ClientId").Value! != ClientId || _configuration.GetSection("AppSettings:ClientSecret").Value != ClientSecret)
                {
                    _logger.LogError($"#### Invalid cline_id or client_secret");
                    return new UnauthorizedObjectResult(new { error = "Invalid client_secret_post credentials" });
                }
            }
            else
            {
                _logger.LogError($"#### Missing cline_id or client_secret");
                return new UnauthorizedObjectResult(new { error = "Invalid client_secret_post credentials" });
            }

            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(code);
                string codeString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                HomeViewModel model = new HomeViewModel();

                // Get the token from the canche using the code
                string id_token = _memoryCache.Get<string>(codeString) ?? "Token not found";

                // Set the token lifetime
                DateTime not_before = DateTime.Now.AddSeconds(-30);
                long not_beforeUnixTime = ((DateTimeOffset)not_before).ToUnixTimeSeconds();

                DateTime expires_on = DateTime.Now.AddDays(7);
                long expires_onUnixTime = ((DateTimeOffset)expires_on).ToUnixTimeSeconds();

                var payload = new
                {
                    access_token = id_token,
                    id_token = id_token,
                    token_type = "bearer",
                    refresh_token = Guid.NewGuid().ToString(),
                    not_before = not_beforeUnixTime,
                    expires_in = 43199,
                    expires_on = expires_onUnixTime,
                    scope = "email openid"
                };

                // Log the response JSON
                _logger.LogInformation($"#### Token response: {JsonSerializer.Serialize(payload)}");

                // Return the JSON payload
                return new OkObjectResult(payload);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"#### Token error: {ex.Message}");
                return new BadRequestObjectResult(ex.Message + " Code: " + code);
            }
        }
    }
}