using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;
using System.Collections.Generic;
using System.Linq;

namespace checkpoint_web.Pages.Fragments
{
 public class UsuariosModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 public UsuariosModel(UserManager<ApplicationUser> userManager) => _userManager = userManager;
 public IList<(string Email, string Nombre, IList<string> Roles)> Usuarios { get; set; } = new List<(string, string, IList<string>)>();
 public async Task OnGetAsync()
 {
 var users = _userManager.Users.ToList();
 foreach(var u in users)
 {
 var roles = await _userManager.GetRolesAsync(u);
 Usuarios.Add((u.Email ?? "", u.UserName ?? "", roles));
 }
 }
 }
}
