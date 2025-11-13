using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Tareas
{
    [Authorize]
    public class MisTareasModel : PageModel
    {
        private readonly ITareaService _tareaService;

     public MisTareasModel(ITareaService tareaService)
        {
            _tareaService = tareaService;
        }

        public IEnumerable<Tarea> TareasPendientes { get; set; } = new List<Tarea>();
        public IEnumerable<Tarea> TareasEnProgreso { get; set; } = new List<Tarea>();
public IEnumerable<Tarea> TareasFinalizadas { get; set; } = new List<Tarea>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();

        public async Task OnGetAsync()
        {
     var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var todasMisTareas = await _tareaService.GetByUsuarioAsync(usuarioId);

            TareasPendientes = todasMisTareas.Where(t => t.Estado == "Pendiente").OrderBy(t => t.FechaLimite);
   TareasEnProgreso = todasMisTareas.Where(t => t.Estado == "EnProgreso").OrderBy(t => t.FechaLimite);
        TareasFinalizadas = todasMisTareas.Where(t => t.Estado == "Finalizada").OrderByDescending(t => t.FechaFinalizacion).Take(10);

  var ahora = DateTime.UtcNow;
    Estadisticas = new Dictionary<string, int>
            {
                ["Total"] = todasMisTareas.Count(),
   ["Pendientes"] = TareasPendientes.Count(),
           ["EnProgreso"] = TareasEnProgreso.Count(),
      ["Finalizadas"] = TareasFinalizadas.Count(),
      ["Vencidas"] = todasMisTareas.Count(t => t.FechaLimite.HasValue && t.FechaLimite.Value < ahora && t.Estado != "Finalizada"),
     ["HoyVencen"] = todasMisTareas.Count(t => t.FechaLimite.HasValue && t.FechaLimite.Value.Date == ahora.Date && t.Estado != "Finalizada")
       };
        }
    }
}
