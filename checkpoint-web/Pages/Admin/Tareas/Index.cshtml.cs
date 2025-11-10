using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ITareaService _tareaService;

public IndexModel(ITareaService tareaService)
 {
    _tareaService = tareaService;
}

  public IEnumerable<Tarea> Tareas { get; set; } = new List<Tarea>();
     public Dictionary<string, int> Estadisticas { get; set; } = new();
        public string? FiltroEstado { get; set; }

      public async Task OnGetAsync(string? estado = null)
        {
     FiltroEstado = estado;

       if (!string.IsNullOrEmpty(estado))
 {
     Tareas = await _tareaService.GetPorEstadoAsync(estado);
            }
      else
      {
            Tareas = await _tareaService.GetAllAsync();
}

            Estadisticas = await _tareaService.GetEstadisticasAsync();
}
    }
}
