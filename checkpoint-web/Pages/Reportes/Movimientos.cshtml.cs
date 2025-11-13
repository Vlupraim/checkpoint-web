using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Reportes
{
    [Authorize]
    public class MovimientosModel : PageModel
    {
        private readonly IReporteService _reporteService;

        public MovimientosModel(IReporteService reporteService)
        {
        _reporteService = reporteService;
        }

     public IEnumerable<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public DateTime? FechaDesde { get; set; }
     public DateTime? FechaHasta { get; set; }
        public string? TipoFiltro { get; set; }

   public async Task OnGetAsync(DateTime? desde = null, DateTime? hasta = null, string? tipo = null)
        {
    FechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
   FechaHasta = hasta ?? DateTime.UtcNow;
     TipoFiltro = tipo;

      Movimientos = await _reporteService.GetMovimientosAsync(FechaDesde, FechaHasta, TipoFiltro);
   }
    }
}
