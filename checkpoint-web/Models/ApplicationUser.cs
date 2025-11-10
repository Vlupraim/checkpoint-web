using Microsoft.AspNetCore.Identity;

namespace checkpoint_web.Models
{
 public class ApplicationUser : IdentityUser
 {
 public string Nombre { get; set; } = string.Empty;
 public bool Activo { get; set; } = true;
 }
}
