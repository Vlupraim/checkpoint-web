using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger) => (_signInManager, _logger) = (signInManager, logger);

        private void TryDeleteCookie(string name)
        {
            try
            {
                var options = new Microsoft.AspNetCore.Http.CookieOptions { Path = "/", HttpOnly = true, Secure = Request.IsHttps };
                Response.Cookies.Delete(name, options);
                _logger.LogInformation("Requested deletion of cookie {cookie}", name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed deleting cookie {cookie}", name);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Logout POST invoked for {user}", User?.Identity?.Name ?? "anonymous");
            await _signInManager.SignOutAsync();
            var known = new[] { "Checkpoint.Auth", "AspNetCore.Identity.Application", ".AspNetCore.Identity.Application", "AspNetCore.Cookies", ".AspNetCore.Cookies" };
            foreach (var c in known) TryDeleteCookie(c);
            _logger.LogInformation("Logout completed and requested cookie deletions");
            return RedirectToPage("/Account/Login");
        }
    }
}
