using Microsoft.AspNetCore.Mvc;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Primitives;
using Microsoft.ApplicationInsights;
using System.Text.Json;

namespace my_idp.oauth2.Controllers
{
    public class UserInfoController : Controller
    {
        private readonly ILogger<UserInfoController> _logger;

        public UserInfoController(ILogger<UserInfoController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation($"#### HTTP GET call to /UserInfo");
            string BearerToken = string.Empty;
            StringValues authorizationHeader;

            // Try to get it from the query string param
            Oauth2TenantConfig settings = new Oauth2TenantConfig();
            if (settings.UserInfo.BearerTokenTransmissionMethod.QueryString)
            {
                BearerToken = this.Request.Query[settings.UserInfo.QueryStringAccessTokenName].ToString() ?? string.Empty;
            }

            // If not found try to get it from the authorization HTTP header
            if (string.IsNullOrEmpty(BearerToken)
                && settings.UserInfo.BearerTokenTransmissionMethod.AuthorizationHeader
                && this.Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                BearerToken = this.Request.Headers["Authorization"].ToString().Split(" ")[1];
            }

            // Check if the bearer token is not found
            if (string.IsNullOrEmpty(BearerToken))
            {
                _logger.LogInformation($"#### Bear token not found");
                return new BadRequestObjectResult(new { error = "Bearer token not found" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.ReadJwtToken(BearerToken);
            List<System.Security.Claims.Claim> claims = ((JwtSecurityToken)token).Claims.ToList();

            Dictionary<string, string> payload = new Dictionary<string, string>();

            foreach (var item in claims)
            {
                payload.Add(item.Type, item.Value);
            }
            
            // Log the payload
            _logger.LogInformation($"#### Payload: {JsonSerializer.Serialize(payload)}");         

            // Return the payload
            return Ok(payload);
        }
    }
}