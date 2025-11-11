using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "Administrador,PersonalBodega,ControlCalidad")]
    public class LotesModel : PageModel
  {
   public void OnGet()
        {
        }
    }
}
