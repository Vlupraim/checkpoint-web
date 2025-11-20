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
                // Intentar eliminar con diferentes opciones para asegurar
                Response.Cookies.Delete(name);
                Response.Cookies.Delete(name, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
                });
                // También intentar con Secure=false por si acaso
                Response.Cookies.Delete(name, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = false
                });
                _logger.LogInformation("Eliminando cookie {cookie}", name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error eliminando cookie {cookie}", name);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Logout POST para {user}", User?.Identity?.Name ?? "anonymous");

            // Primero hacer SignOut de Identity
            await _signInManager.SignOutAsync();

            // Luego eliminar explícitamente TODAS las cookies conocidas (viejas y nuevas)
            var known = new[]
            {
                // Cookies nuevas
                "Checkpoint.Session",
                // Cookies viejas
                "Checkpoint.Auth",
                "CheckpointAuth",
                "AspNetCore.Identity.Application",
                ".AspNetCore.Identity.Application",
                "AspNetCore.Cookies",
                ".AspNetCore.Cookies",
                "CheckpointAntiforgery",
                ".AspNetCore.Antiforgery"
            };

            foreach (var c in known)
            {
                TryDeleteCookie(c);
            }

            _logger.LogInformation("Logout completado - todas las cookies eliminadas");
            return RedirectToPage("/Account/Login");
        }

        // También permitir logout por GET para facilitar limpieza
        public async Task<IActionResult> OnGetAsync()
        {
            return await OnPostAsync();
        }
    }
}
