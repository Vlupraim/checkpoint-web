using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega,ControlCalidad")]
    public class StockModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
