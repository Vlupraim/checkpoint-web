using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "Administrador,PersonalBodega")]
 public class MovimientosModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
