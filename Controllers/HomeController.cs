using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using my_idp.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;

namespace my_idp.oauth2.Controllers
{
    public class HomeController : Controller
    {
        Random rand = new Random();
        private static Lazy<X509SigningCredentials> SigningCredentials = null!;
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            SigningCredentials = Commons.LoadCertificate(_configuration.GetSection("AppSettings:SigningCertThumbprint").Value!);
        }

        [ActionName("index")]
        public IActionResult Index()
        {
            ViewData["client_id"] = string.IsNullOrEmpty(this.Request.Query["client_id"].ToString()) ? "1234" : this.Request.Query["client_id"].ToString();
            ViewData["scope"] = string.IsNullOrEmpty(this.Request.Query["scope"].ToString()) ? "read" : this.Request.Query["scope"].ToString();
            ViewData["redirect_uri"] = string.IsNullOrEmpty(this.Request.Query["redirect_uri"].ToString()) ? "https://jwt.ms/#" : this.Request.Query["redirect_uri"].ToString();
            ViewData["state"] = string.IsNullOrEmpty(this.Request.Query["state"].ToString()) ? "xyz" : this.Request.Query["state"].ToString();

            return View();
        }

        [HttpPost]
        [ActionName("index")]
        public RedirectResult SignIn(HomeViewModel model)
        {

            string code = $"{model.client_id}|{model.email}";
            var codeTextBytes = System.Text.Encoding.UTF8.GetBytes(code);
            string URL = $"{model.redirect_uri}?code={System.Convert.ToBase64String(codeTextBytes)}";

            // Generate the JWT token
            string id_token = Commons.BuildJwtToken(HomeController.SigningCredentials.Value, _configuration.GetSection("AppSettings:issuer").Value!, this.Request, model);

            // Return the state parameter
            URL += $"&state={(model.state ?? string.Empty).Replace("=", "%3D")}";

            if (model.client_id == "default")
                URL = URL + $"&id_token={id_token}";

            // Add the token to the cache
            _memoryCache.Set(code, id_token);

            // Redirect to the client
            return Redirect(URL);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}