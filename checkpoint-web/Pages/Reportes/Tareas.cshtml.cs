using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<IActionResult> OnGetAsync(DateTime? desde = null, DateTime? hasta = null, string? estado = null)
        {
            try
            {
                FechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
                FechaHasta = hasta ?? DateTime.UtcNow;
                EstadoFiltro = estado;

                Tareas = await _reporteService.GetTareasAsync(EstadoFiltro, FechaDesde, FechaHasta);
                return Page();
            }
            catch (Exception ex)
            {
                // Log the error and show a friendly message
                TempData["ErrorMessage"] = $"Error al cargar el reporte de tareas: {ex.Message}";
                Tareas = new List<Tarea>();
                return Page();
            }
        }
    }
}
