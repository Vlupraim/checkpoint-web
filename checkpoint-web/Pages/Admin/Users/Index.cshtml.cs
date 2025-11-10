using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;

namespace checkpoint_web.Pages.Admin.Users
{
 [Authorize(Roles = "Administrador")]
 public class IndexModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;
 public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 }

 public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
 public Dictionary<string, IList<string>> RolesForUser { get; set; } = new Dictionary<string, IList<string>>();

 public async Task OnGetAsync()
 {
 Users = _userManager.Users.ToList();
 foreach (var u in Users)
 {
 var roles = await _userManager.GetRolesAsync(u);
 RolesForUser[u.Id] = roles;
 }
 }
 }
}
