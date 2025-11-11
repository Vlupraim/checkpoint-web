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
 // CORREGIDO: Cambiado de Email+Nombre a Id+Email+Nombre para usar UserId en las rutas
 public IList<(string Id, string Email, string Nombre, IList<string> Roles)> Usuarios { get; set; } = new List<(string, string, string, IList<string>)>();
 public async Task OnGetAsync()
 {
 var users = _userManager.Users.ToList();
 foreach(var u in users)
 {
 var roles = await _userManager.GetRolesAsync(u);
 Usuarios.Add((u.Id, u.Email ?? "", u.UserName ?? "", roles));
 }
 }
 }
}
