using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;

namespace my_idp.oauth2.Controllers
{
    [Area("oauth2")]
    public class LogoutController : Controller
    {
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(ILogger<LogoutController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public string Index()
        {
            // Get the tenant settings
            return "Logout place holder.";
        }
    }
}