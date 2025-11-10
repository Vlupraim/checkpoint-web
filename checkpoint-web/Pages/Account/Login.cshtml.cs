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
            
      if (!ModelState.IsValid)
   {
            return Page();
      }

      _logger.LogInformation("Login attempt for {email}", Email);
   
            // Ensure clean state
  await _signInManager.SignOutAsync();
        
            var user = await _userManager.FindByEmailAsync(Email);
     if (user == null)
    {
       ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
  return Page();
         }

         var result = await _signInManager.CheckPasswordSignInAsync(user, Password, lockoutOnFailure: false);
        if (!result.Succeeded)
    {
    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
         return Page();
    }

// Sign in with session-only cookie (expires when browser closes)
  // DO NOT set IsPersistent = true, this would create a persistent cookie
       await _signInManager.SignInAsync(user, isPersistent: false);

    var roles = await _userManager.GetRolesAsync(user);
   _logger.LogInformation("User {email} signed in successfully with session-only cookie. Roles: {roles}", 
        Email, string.Join(", ", roles));

    // Redirect based on role
   if (roles.Contains("Administrador"))
        {
     return LocalRedirect("/Admin/Dashboard");
            }
        else if (roles.Contains("PersonalBodega"))
         {
  return LocalRedirect("/Bodega/Dashboard");
            }
            else if (roles.Contains("ControlCalidad"))
       {
    return LocalRedirect("/Calidad/Dashboard");
       }

            return LocalRedirect(ReturnUrl);
        }
    }
}
