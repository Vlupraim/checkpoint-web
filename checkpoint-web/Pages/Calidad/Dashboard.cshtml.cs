using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Calidad
{
    [Authorize(Roles = "Administrador,ControlCalidad")]
  public class DashboardModel : PageModel
    {
        private readonly ICalidadService _calidadService;

     public DashboardModel(ICalidadService calidadService)
 {
  _calidadService = calidadService;
    }

     public Dictionary<string, int>? Estadisticas { get; set; }

 public async Task OnGetAsync()
        {
       Estadisticas = await _calidadService.GetEstadisticasAsync();
        }
    }
}
