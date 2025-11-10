using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class DashboardModel : PageModel
 {
 private readonly IReporteService _reporteService;
 private readonly INotificacionService _notificacionService;

     public DashboardModel(IReporteService reporteService, INotificacionService notificacionService)
  {
 _reporteService = reporteService;
    _notificacionService = notificacionService;
  }

 public ResumenOperativo? Resumen { get; set; }
 public InventarioReporte? Inventario { get; set; }

        public async Task OnGetAsync()
     {
 Resumen = await _reporteService.GetResumenOperativoAsync();
Inventario = await _reporteService.GetInventarioActualAsync();
  
  // Generar notificaciones automáticas
        await _notificacionService.GenerarNotificacionesAutomaticasAsync();
     }
    }
}
