using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Calidad
{
    [Authorize(Roles = "Administrador,ControlCalidad")]
    public class DashboardModel : PageModel
    {
        private readonly ICalidadService _calidadService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            ICalidadService calidadService,
            ILogger<DashboardModel> logger)
        {
            _calidadService = calidadService;
            _logger = logger;
        }

        public Dictionary<string, int>? Estadisticas { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("[CALIDAD] Loading dashboard for {user}", User.Identity?.Name);

                Estadisticas = await _calidadService.GetEstadisticasAsync();

                _logger.LogInformation("[CALIDAD] Dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CALIDAD] Error loading dashboard");
                ErrorMessage = "Error al cargar el dashboard. Algunos datos pueden no estar disponibles.";
                // No lanzar excepción - mostrar dashboard con mensaje de error
            }
        }
    }
}
