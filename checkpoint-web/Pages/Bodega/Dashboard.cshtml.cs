using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega")]
    public class DashboardModel : PageModel
    {
        private readonly IReporteService _reporteService;
        private readonly IMovimientoService _movimientoService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IReporteService reporteService, 
            IMovimientoService movimientoService,
            ILogger<DashboardModel> logger)
        {
            _reporteService = reporteService;
            _movimientoService = movimientoService;
            _logger = logger;
        }

        public ResumenOperativo? Resumen { get; set; }
        public Dictionary<string, object>? EstadisticasMovimientos { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("[BODEGA] Loading dashboard for {user}", User.Identity?.Name);
 
                Resumen = await _reporteService.GetResumenOperativoAsync();
                EstadisticasMovimientos = await _movimientoService.GetEstadisticasAsync();
    
                _logger.LogInformation("[BODEGA] Dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BODEGA] Error loading dashboard");
                ErrorMessage = "Error al cargar el dashboard. Algunos datos pueden no estar disponibles.";
                // No lanzar excepción - mostrar dashboard con mensaje de error
            }
        }
    }
}
