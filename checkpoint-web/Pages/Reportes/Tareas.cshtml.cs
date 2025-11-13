using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Reportes
{
    [Authorize]
    public class TareasModel : PageModel
    {
        private readonly IReporteService _reporteService;

        public TareasModel(IReporteService reporteService)
        {
    _reporteService = reporteService;
        }

        public IEnumerable<Tarea> Tareas { get; set; } = new List<Tarea>();
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? EstadoFiltro { get; set; }

    public async Task OnGetAsync(DateTime? desde = null, DateTime? hasta = null, string? estado = null)
        {
      FechaDesde = desde ?? DateTime.Now.AddDays(-30);
   FechaHasta = hasta ?? DateTime.Now;
  EstadoFiltro = estado;

   Tareas = await _reporteService.GetTareasAsync(EstadoFiltro, FechaDesde, FechaHasta);
        }
    }
}
