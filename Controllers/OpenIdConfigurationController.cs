using System;
using my_idp.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace my_idp.oauth2.Controllers
{
    public class OpenIdConfigurationController : Controller
    {
        private static Lazy<X509SigningCredentials> SigningCredentials = null!;
        private readonly ILogger<OpenIdConfigurationController> _logger;
        private readonly IConfiguration _configuration;

        public OpenIdConfigurationController(ILogger<OpenIdConfigurationController> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
            SigningCredentials = Commons.LoadCertificate(_configuration.GetSection("AppSettings:SigningCertThumbprint").Value!);
        }

        public IActionResult Index()
        {

            OidcConfigurationModel payload = new OidcConfigurationModel
            {
                // The issuer name
                Issuer = _configuration.GetSection("AppSettings:issuer").Value!,

                // Include the absolute URL to JWKs endpoint
                JwksUri = Url.Link("oidc-jwks", new { })!,

                // End points
                AuthorizationEndpoint = Url.ActionLink("Index", "home")!.Replace("http://", "https://"),
                TokenEndpoint = Url.ActionLink("Index", "token")!.Replace("http://", "https://"),
                UserInfoEndpoint = Url.ActionLink("Index", "userinfo")!.Replace("http://", "https://"),
                EndSessionEndpoint = Url.ActionLink("Index", "logout")!.Replace("http://", "https://"),

                // Other metadata
                response_modes_supported = new[] { "query", "fragment", "form_post" },
                response_types_supported = new[] { "code", "code id_token", "id_token", "id_token token" },
                scopes_supported = new[] { "openid", "profile", "offline_access" },
                token_endpoint_auth_methods_supported = new[] { "client_secret_post", "private_key_jwt", "client_secret_basic" },
                claims_supported = new[] { "sub", "name", "email", "email_verified", "nbf", "exp", "iss", "aud", "iat", "auth_time", "acr", "nonce" },
                subject_types_supported = new[] { "pairwise" },

                // Include the supported signing algorithms
                IdTokenSigningAlgValuesSupported = new[] { OpenIdConfigurationController.SigningCredentials.Value.Algorithm }
            };

            return Ok(payload);
        }
    }
}