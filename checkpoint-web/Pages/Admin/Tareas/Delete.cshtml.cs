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
            try
            {
                var result = await _tareaService.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar la tarea (no encontrada).";
                    return RedirectToPage("./Index");
                }

                TempData["SuccessMessage"] = "Tarea eliminada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Log if required
                TempData["ErrorMessage"] = "Error al eliminar la tarea: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }
    }
}
