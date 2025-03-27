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
        public IActionResult IndexAsyncGet()
        {
            _logger.LogInformation($"#### HTTP GET call to /toekn");

            return IndexCommonAsync();
        }

        [HttpPost]
        [ActionName("index")]
        public IActionResult IndexAsyncPost()
        {
            _logger.LogInformation($"#### HTTP POST call to /toekn");

            // Log full request URL including query string
            var fullUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            _logger.LogInformation($"#### Full request URL: {fullUrl}");

            // Log all headers in one line as JSON
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            _logger.LogInformation($"#### HTTP request Headers: {JsonSerializer.Serialize(headers)}");

            // Log application/x-www-form-urlencoded data in one line as JSON
            var formData = Request.Form.ToDictionary(f => f.Key, f => f.Value.ToString());
            _logger.LogInformation($"#### HTTP request Form data: {JsonSerializer.Serialize(formData)}");


            return IndexCommonAsync();
        }

        private IActionResult IndexCommonAsync()
        {

            string clientId, clientSecret, grantType, userIDBase64 = string.Empty;

            // Get the client_id, client_secret, and grant_type from the request
            if (Request.Method == "POST")
            {
                clientId = this.Request.Form["client_id"].ToString() ?? string.Empty;
                clientSecret = this.Request.Form["client_secret"].ToString() ?? string.Empty;
                grantType = this.Request.Form["grant_type"].ToString() ?? string.Empty;

                // Check if the grant type is authorization_code or refresh_token
                // and get the userIDBase64 from the code or refresh_token parameter
                if (grantType == "authorization_code")
                {
                    userIDBase64 = this.Request.Form["code"].ToString() ?? string.Empty;
                }
                else if (grantType == "refresh_token")
                {
                    userIDBase64 = this.Request.Form["refresh_token"].ToString() ?? string.Empty;
                }
                else
                {
                    _logger.LogError($"#### Invalid grant type: {grantType}");
                    return new BadRequestObjectResult(new { error = "Invalid grant type." });
                }    
            }
            else
            {
                // For GET requests, get the client_id and client_secret from the query string
                clientId = this.Request.Query["client_id"].ToString() ?? string.Empty;
                clientSecret = this.Request.Query["client_secret"].ToString() ?? string.Empty;
            }

            // Check if the client_id and client_secret are present
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                // Check if the client_id and client_secret match the expected values
                Oauth2TenantConfig settings = new Oauth2TenantConfig();
                if (_configuration.GetSection("AppSettings:ClientId").Value! != clientId || _configuration.GetSection("AppSettings:ClientSecret").Value != clientSecret)
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
                // Get the token from the canche using the code
                string id_token = _memoryCache.Get<string>(userIDBase64) ?? "Token not found";

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
                return new BadRequestObjectResult(ex.Message + " Code: " + userIDBase64);
            }
        }
    }
}