using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;

namespace checkpoint_web.Pages.Admin.Users
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : checkpoint_web.Pages.Fragments.UsuariosModel
    {
        public IndexModel(UserManager<ApplicationUser> userManager) : base(userManager)
        {
        }
    }
}
