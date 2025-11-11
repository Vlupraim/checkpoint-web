using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Calidad
{
  [Authorize(Roles = "Administrador,ControlCalidad")]
    public class HistorialModel : PageModel
    {
        public void OnGet()
  {
        }
    }
}
