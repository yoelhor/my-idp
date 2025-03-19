using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;

namespace my_idp.oauth2.Controllers
{
    public class AuthorizeController : Controller
    {
        Random rand = new Random();
        private static Lazy<X509SigningCredentials> SigningCredentials = null!;
        private readonly ILogger<AuthorizeController> _logger;

        public AuthorizeController(ILogger<AuthorizeController> logger)
        {
            _logger = logger;
            SigningCredentials = Commons.LoadCertificate();
        }

        [ActionName("index")]
        public IActionResult Index()
        {
            ViewData["client_id"] = string.IsNullOrEmpty(this.Request.Query["client_id"].ToString()) ? "default" : this.Request.Query["client_id"].ToString();
            ViewData["scope"] = string.IsNullOrEmpty(this.Request.Query["scope"].ToString()) ? "read" : this.Request.Query["scope"].ToString();
            ViewData["redirect_uri"] = string.IsNullOrEmpty(this.Request.Query["redirect_uri"].ToString()) ? "https://jwt.ms/#" : this.Request.Query["redirect_uri"].ToString();
            ViewData["state"] = string.IsNullOrEmpty(this.Request.Query["state"].ToString()) ? "abcd" : this.Request.Query["state"].ToString();

            return View();
        }

        [HttpPost]
        [ActionName("index")]
        public RedirectResult SignIn(string email, string password, string displayName, string redirect_uri, string client_id, string state)
        {

            string code = $"{rand.Next(12345, 99999)}|{client_id}|{email}";
            var codeTextBytes = System.Text.Encoding.UTF8.GetBytes(code);
            string id_token = Commons.BuildJwtToken(AuthorizeController.SigningCredentials.Value, this.Request, client_id, displayName, email);
            //id_token={id_token}&
            string URL = $"{redirect_uri}?code={System.Convert.ToBase64String(codeTextBytes)}";

            // Return the state parameter
            URL += $"&state={state.Replace("=", "%3D")}";

            if (client_id == "default")
                URL = URL + $"&id_token={id_token}";

            return Redirect(URL);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}