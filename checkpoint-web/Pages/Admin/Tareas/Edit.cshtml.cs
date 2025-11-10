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

     public async Task<IActionResult> OnGetAsync(int id)
     {
  var tarea = await _tareaService.GetByIdAsync(id);
    if (tarea == null)
      return NotFound();

   Tarea = tarea;
     LoadSelectLists();
      return Page();
   }

        public async Task<IActionResult> OnPostAsync()
        {
   if (!ModelState.IsValid)
   {
         LoadSelectLists();
   return Page();
  }

      await _tareaService.UpdateAsync(Tarea);
   TempData["SuccessMessage"] = "Tarea actualizada exitosamente";
  return RedirectToPage("./Index");
}

        private void LoadSelectLists()
        {
     EstadosSelectList = new SelectList(new[] { "Pendiente", "EnProgreso", "Finalizada", "Cancelada", "Bloqueada" });
  PrioridadesSelectList = new SelectList(new[] { "Baja", "Media", "Alta", "Urgente" });
        }
    }
}
