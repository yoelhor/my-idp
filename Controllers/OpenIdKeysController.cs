using System;
using my_idp.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace my_idp.oauth2.Controllers
{
    public class OpenIdKeysController : Controller
    {
        private static Lazy<X509SigningCredentials> SigningCredentials = null!;
        private readonly ILogger<OpenIdKeysController> _logger;

        public OpenIdKeysController(ILogger<OpenIdKeysController> logger)
        {
            _logger = logger;
            SigningCredentials = Commons.LoadCertificate();
        }

        public ActionResult Index()
        {
            try
            {
                JwksModel payload = new JwksModel
                {
                    Keys = new[] { JwksKeyModel.FromSigningCredentials(OpenIdKeysController.SigningCredentials.Value) }
                };

                return Ok(payload);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}