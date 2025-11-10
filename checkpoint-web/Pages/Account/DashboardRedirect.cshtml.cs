using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint_web.Pages.Account
{
 public class DashboardRedirectModel : PageModel
 {
 public IActionResult OnGet()
 {
 if (!User.Identity?.IsAuthenticated ?? false)
 {
 return RedirectToPage("/Account/Login");
 }
 if (User.IsInRole("Administrador")) return RedirectToPage("/Admin/Dashboard");
 if (User.IsInRole("PersonalBodega")) return RedirectToPage("/Bodega/Dashboard");
 if (User.IsInRole("ControlCalidad")) return RedirectToPage("/Calidad/Dashboard");
 // default
 return RedirectToPage("/Account/Login");
 }
 }
}
