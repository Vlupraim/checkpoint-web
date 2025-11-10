using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;

namespace checkpoint_web.Pages.Admin.Users
{
 [Authorize(Roles = "Administrador")]
 public class EditRolesModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;
 public EditRolesModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 }

 [BindProperty]
 public string UserId { get; set; } = string.Empty;
 [BindProperty]
 public List<string> Roles { get; set; } = new List<string>();
 public IList<IdentityRole> AllRoles { get; set; } = new List<IdentityRole>();
 public string Email { get; set; } = string.Empty;

 public async Task<IActionResult> OnGetAsync(string id)
 {
 if (string.IsNullOrEmpty(id)) return NotFound();
 var user = await _userManager.FindByIdAsync(id);
 if (user == null) return NotFound();
 UserId = id;
 Email = user.Email ?? string.Empty;
 AllRoles = _roleManager.Roles.ToList();
 var userRoles = await _userManager.GetRolesAsync(user);
 Roles = userRoles.ToList();
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 var user = await _userManager.FindByIdAsync(UserId);
 if (user == null) return NotFound();
 var currentRoles = await _userManager.GetRolesAsync(user);
 var toAdd = Roles.Except(currentRoles);
 var toRemove = currentRoles.Except(Roles);
 await _userManager.AddToRolesAsync(user, toAdd);
 await _userManager.RemoveFromRolesAsync(user, toRemove);
 return RedirectToPage("Index");
 }
 }
}
