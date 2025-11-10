using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
   private readonly ITareaService _tareaService;

        public DeleteModel(ITareaService tareaService)
     {
  _tareaService = tareaService;
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = new();

      public async Task<IActionResult> OnGetAsync(int id)
        {
      var tarea = await _tareaService.GetByIdAsync(id);
  if (tarea == null)
     return NotFound();

         Tarea = tarea;
       return Page();
    }

        public async Task<IActionResult> OnPostAsync(int id)
        {
      await _tareaService.DeleteAsync(id);
       TempData["SuccessMessage"] = "Tarea eliminada exitosamente";
return RedirectToPage("./Index");
        }
    }
}
