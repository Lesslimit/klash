using AspNet.Security.OAuth.Slack;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        [HttpPost("~/signin")]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, SlackAuthenticationDefaults.AuthenticationScheme);
        }
    }
}