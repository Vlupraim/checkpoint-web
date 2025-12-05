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
                // Guardar los filtros para mostrarlos en la vista
                FechaDesde = desde;
                FechaHasta = hasta;
                EstadoFiltro = estado;

                // Si no se especifican fechas, NO aplicar filtro de fecha (mostrar TODAS las tareas)
                DateTime? filtroDesde = desde;
                DateTime? filtroHasta = hasta;

                // Solo si el usuario especificó fechas, ajustar el hasta para incluir el día completo
                if (hasta.HasValue)
                {
                    filtroHasta = hasta.Value.Date.AddDays(1).AddSeconds(-1);
                }

                Tareas = await _reporteService.GetTareasAsync(EstadoFiltro, filtroDesde, filtroHasta);
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
