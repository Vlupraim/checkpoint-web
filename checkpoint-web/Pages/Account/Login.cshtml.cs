using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace checkpoint_web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger) 
 => (_signInManager, _userManager, _logger) = (signInManager, userManager, logger);

        [BindProperty]
        public string Email { get; set; } = string.Empty;
    [BindProperty]
  public string Password { get; set; } = string.Empty;
      [BindProperty]
        public bool RememberMe { get; set; } = false; // Nuevo: checkbox "Recuérdame"
   public string ReturnUrl { get; set; } = string.Empty;

        public async Task OnGetAsync(string? returnUrl = null)
 {
// Clear any existing session
     await _signInManager.SignOutAsync();
  ReturnUrl = returnUrl ?? Url.Content("~/");
 }

 public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
 {
    ReturnUrl = returnUrl ?? Url.Content("~/");
  
    _logger.LogInformation("[LOGIN] Starting login attempt for {email}, ReturnUrl: {returnUrl}, RememberMe: {rememberMe}", 
        Email, ReturnUrl, RememberMe);
    
    if (!ModelState.IsValid)
   {
  _logger.LogWarning("[LOGIN] ModelState invalid for {email}", Email);
            return Page();
      }

      _logger.LogInformation("[LOGIN] Login attempt for {email}", Email);
   
 // Ensure clean state
  await _signInManager.SignOutAsync();
    _logger.LogInformation("[LOGIN] Signed out any existing session for {email}", Email);
    
         var user = await _userManager.FindByEmailAsync(Email);
     if (user == null)
    {
      _logger.LogWarning("[LOGIN] User not found: {email}", Email);
    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
return Page();
}

    _logger.LogInformation("[LOGIN] User found: {email}, checking password...", Email);
    
         var result = await _signInManager.CheckPasswordSignInAsync(user, Password, lockoutOnFailure: false);
        if (!result.Succeeded)
    {
    _logger.LogWarning("[LOGIN] Password check failed for {email}. Result: {result}", Email, result);
    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
         return Page();
    }

    _logger.LogInformation("[LOGIN] Password correct for {email}, signing in...", Email);

    // CRÍTICO: isPersistent controla si la cookie es temporal o persistente
    // - false (sin RememberMe): Cookie de sesión, expira en 10 minutos de inactividad
    // - true (con RememberMe): Cookie persistente, dura 30 días
    await _signInManager.SignInAsync(user, isPersistent: RememberMe);

    var roles = await _userManager.GetRolesAsync(user);
    var sessionType = RememberMe ? "persistent cookie (30 days)" : "session cookie (10 min timeout)";
   _logger.LogInformation("[LOGIN] User {email} signed in successfully with {sessionType}. Roles: {roles}", 
        Email, sessionType, string.Join(", ", roles));

    // Redirect based on role
    string redirectUrl;
  if (roles.Contains("Administrador"))
    {
 redirectUrl = "/Admin/Dashboard";
 }
    else if (roles.Contains("PersonalBodega"))
    {
        redirectUrl = "/Bodega/Dashboard";
    }
    else if (roles.Contains("ControlCalidad"))
    {
    redirectUrl = "/Calidad/Dashboard";
    }
    else
    {
        redirectUrl = ReturnUrl;
    }
    
    _logger.LogInformation("[LOGIN] Redirecting {email} to {url}", Email, redirectUrl);
    return LocalRedirect(redirectUrl);
}
    }
}
