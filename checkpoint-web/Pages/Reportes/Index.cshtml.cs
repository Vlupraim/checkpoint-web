using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Reportes
{
    [Authorize(Roles = "Administrador,PersonalBodega,ControlCalidad")]
   public class IndexModel : PageModel
    {
 private readonly IReporteService _reporteService;

        public IndexModel(IReporteService reporteService)
    {
  _reporteService = reporteService;
   }

      public InventarioReporte? InventarioActual { get; set; }
 public ResumenOperativo? ResumenHoy { get; set; }

        public async Task OnGetAsync()
        {
 InventarioActual = await _reporteService.GetInventarioActualAsync();
    ResumenHoy = await _reporteService.GetResumenOperativoAsync();
  }
    }
}
