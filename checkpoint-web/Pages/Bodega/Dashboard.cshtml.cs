using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "Administrador,PersonalBodega")]
    public class DashboardModel : PageModel
    {
        private readonly IReporteService _reporteService;
        private readonly IMovimientoService _movimientoService;

        public DashboardModel(IReporteService reporteService, IMovimientoService movimientoService)
        {
            _reporteService = reporteService;
            _movimientoService = movimientoService;
        }

        public ResumenOperativo? Resumen { get; set; }
        public Dictionary<string, object>? EstadisticasMovimientos { get; set; }

        public async Task OnGetAsync()
        {
            Resumen = await _reporteService.GetResumenOperativoAsync();
            EstadisticasMovimientos = await _movimientoService.GetEstadisticasAsync();
        }
    }
}
