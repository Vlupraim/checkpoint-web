using Microsoft.AspNetCore.Mvc;
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
    public class EditModel : PageModel
    {
     private readonly ITareaService _tareaService;
        private readonly CheckpointDbContext _context;

  public EditModel(ITareaService tareaService, CheckpointDbContext context)
  {
          _tareaService = tareaService;
    _context = context;
  }

        [BindProperty]
public Tarea Tarea { get; set; } = new();
  public SelectList? EstadosSelectList { get; set; }
        public SelectList? PrioridadesSelectList { get; set; }
        public SelectList? UsuariosSelectList { get; set; }

     public async Task<IActionResult> OnGetAsync(int id)
        {
  var tarea = await _tareaService.GetByIdAsync(id);
    if (tarea == null)
            return NotFound();

   Tarea = tarea;
    await LoadSelectListsAsync();
      return Page();
        }

     public async Task<IActionResult> OnPostAsync()
     {
       if (!ModelState.IsValid)
      {
      await LoadSelectListsAsync();
     return Page();
  }

    await _tareaService.UpdateAsync(Tarea);
     TempData["SuccessMessage"] = "Tarea actualizada exitosamente";
  return RedirectToPage("./Index");
      }

        private async Task LoadSelectListsAsync()
     {
  EstadosSelectList = new SelectList(new[] { "Pendiente", "EnProgreso", "Finalizada", "Cancelada", "Bloqueada" });
    PrioridadesSelectList = new SelectList(new[] { "Baja", "Media", "Alta", "Urgente" });
    
 // Cargar usuarios activos
       var usuarios = await _context.Users
   .Where(u => u.Activo)
    .OrderBy(u => u.Nombre)
       .Select(u => new { u.Id, Display = u.Nombre + " (" + u.Email + ")" })
   .ToListAsync();
      UsuariosSelectList = new SelectList(usuarios, "Id", "Display");
        }
    }
}
