using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using checkpoint_web.Models;

namespace checkpoint_web.Pages.Calidad
{
    [Authorize(Roles = "ControlCalidad")]
    public class ControlCalidadModel : PageModel
    {
        private readonly ICalidadService _calidadService;

        public ControlCalidadModel(ICalidadService calidadService)
        {
            _calidadService = calidadService;
        }

        public IEnumerable<Lote> LotesPendientes { get; set; } = new List<Lote>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();

        public async Task OnGetAsync()
        {
            LotesPendientes = await _calidadService.GetLotesPendientesRevisionAsync();
            Estadisticas = await _calidadService.GetEstadisticasAsync();
        }
    }
}
