using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class DashboardModel : PageModel
    {
        private readonly IReporteService _reporteService;
        private readonly INotificacionService _notificacionService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IReporteService reporteService,
            INotificacionService notificacionService,
            ILogger<DashboardModel> logger)
        {
            _reporteService = reporteService;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        public ResumenOperativo? Resumen { get; set; }
        public InventarioReporte? Inventario { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("[DASHBOARD] Loading dashboard for {user}", User.Identity?.Name);

                Resumen = await _reporteService.GetResumenOperativoAsync();
                Inventario = await _reporteService.GetInventarioActualAsync();

                // Generar notificaciones automáticas
                await _notificacionService.GenerarNotificacionesAutomaticasAsync();

                _logger.LogInformation("[DASHBOARD] Dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DASHBOARD] Error loading dashboard");
                ErrorMessage = "Error al cargar el dashboard. Algunos datos pueden no estar disponibles.";
                // No lanzar excepción - mostrar dashboard con mensaje de error
            }
        }
    }
}
