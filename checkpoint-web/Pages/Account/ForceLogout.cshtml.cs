using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace checkpoint_web.Pages.Account
{
    public class ForceLogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ForceLogoutModel> _logger;
        public ForceLogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<ForceLogoutModel> logger) => (_signInManager, _logger) = (signInManager, logger);

        private void AppendExpiredCookieHeader(string name)
        {
            try
            {
                var expires = "Thu, 01 Jan 1970 00:00:00 GMT";
                var parts = new List<string> { $"{name}=; Expires={expires}", "Path=/" };
                if (Request.IsHttps)
                {
                    parts.Add("SameSite=None");
                    parts.Add("Secure");
                }
                else
                {
                    parts.Add("SameSite=Lax");
                }
                var headerValue = string.Join("; ", parts);
                Response.Headers.Append("Set-Cookie", headerValue);
                _logger.LogInformation("Appended expired Set-Cookie for {cookie}: {header}", name, headerValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed appending expired Set-Cookie for {cookie}", name);
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("ForceLogout invoked for {user}", User?.Identity?.Name ?? "anonymous");
            try
            {
                // log incoming request cookies
                var cookies = Request.Cookies.Keys.ToList();
                _logger.LogInformation("Incoming request cookies: {cookies}", string.Join(',', cookies));

                await _signInManager.SignOutAsync();
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Known cookie names to clear
                var known = new[] { "Checkpoint.Auth", "AspNetCore.Identity.Application", ".AspNetCore.Identity.Application", "AspNetCore.Cookies", ".AspNetCore.Cookies" };
                foreach (var name in known)
                {
                    try { Response.Cookies.Delete(name); _logger.LogInformation("Deleted cookie response for {name}", name); } catch (Exception ex) { _logger.LogWarning(ex, "Failed deleting cookie {name}", name); }
                    AppendExpiredCookieHeader(name);
                }

                // Also expire every cookie the browser sent in the request (helps if names differ)
                foreach (var name in cookies)
                {
                    try { Response.Cookies.Delete(name); } catch { }
                    AppendExpiredCookieHeader(name);
                }

                // Log response Set-Cookie headers
                if (Response.Headers.ContainsKey("Set-Cookie"))
                {
                    var setCookies = Response.Headers["Set-Cookie"].ToArray();
                    _logger.LogInformation("Response will set cookies: {setCookies}", string.Join(';', setCookies));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during ForceLogout");
            }

            // Render page that will run client-side JS to clear cookies and redirect
            return Page();
        }
    }
}
