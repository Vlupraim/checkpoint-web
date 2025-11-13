using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
  public class IndexModel : PageModel
    {
     private readonly ITareaService _tareaService;
   private readonly CheckpointDbContext _context;

        public IndexModel(ITareaService tareaService, CheckpointDbContext context)
        {
            _tareaService = tareaService;
   _context = context;
        }

  public IEnumerable<Tarea> Tareas { get; set; } = new List<Tarea>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();
        public string? FiltroEstado { get; set; }
   public string? FiltroResponsable { get; set; }
        public SelectList? UsuariosSelectList { get; set; }

   // Map userId -> display name (Nombre (email))
   public Dictionary<string, string> UserNames { get; set; } = new Dictionary<string, string>();

   public async Task OnGetAsync(string? estado = null, string? responsable = null)
    {
   FiltroEstado = estado;
            FiltroResponsable = responsable;

   var todasTareas = await _tareaService.GetAllAsync();

  // Aplicar filtros
  if (!string.IsNullOrEmpty(estado))
      {
 todasTareas = todasTareas.Where(t => t.Estado == estado);
            }

   if (!string.IsNullOrEmpty(responsable))
       {
        todasTareas = todasTareas.Where(t => t.ResponsableId == responsable);
 }

   Tareas = todasTareas;
   Estadisticas = await _tareaService.GetEstadisticasAsync();

      // Cargar usuarios para el filtro
     var usuarios = await _context.Users
  .Where(u => u.Activo)
 .OrderBy(u => u.Nombre)
    .Select(u => new { u.Id, Display = u.Nombre + " (" + u.Email + ")" })
  .ToListAsync();
     UsuariosSelectList = new SelectList(usuarios, "Id", "Display");

     // Build UserNames dictionary for display in table
 foreach (var u in usuarios)
 {
 if (!string.IsNullOrEmpty(u.Id) && !UserNames.ContainsKey(u.Id))
 {
 UserNames[u.Id] = u.Display;
 }
 }
        }
    }
}
