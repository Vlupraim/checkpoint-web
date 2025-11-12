using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using checkpoint_web.Models;

namespace checkpoint_web.Pages.Calidad
{
  [Authorize(Roles = "ControlCalidad")]
    public class HistorialModel : PageModel
    {
    private readonly ICalidadService _calidadService;

        public HistorialModel(ICalidadService calidadService)
   {
     _calidadService = calidadService;
        }

    public IEnumerable<CalidadLiberacion> Historial { get; set; } = new List<CalidadLiberacion>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();

        public async Task OnGetAsync()
  {
            Historial = await _calidadService.GetHistorialCalidadAsync(100);
Estadisticas = await _calidadService.GetEstadisticasAsync();
        }
    }
}
