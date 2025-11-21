using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ITareaService tareaService, ILogger<DeleteModel> logger)
        {
            _tareaService = tareaService;
            _logger = logger;
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var tarea = await _tareaService.GetByIdAsync(id);
                if (tarea == null)
                {
                    _logger.LogWarning("OnGetAsync Delete: tarea {TareaId} no encontrada", id);
                    return NotFound();
                }

                Tarea = tarea;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnGetAsync Delete failed for id {TareaId}", id);
                TempData["ErrorMessage"] = "Error al cargar la tarea: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete tarea {TareaId} by user {User}", id, User?.Identity?.Name ?? "anonymous");
                var result = await _tareaService.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Delete failed - tarea {TareaId} not found", id);
                    TempData["ErrorMessage"] = "No se pudo eliminar la tarea (no encontrada).";
                    return RedirectToPage("./Index");
                }

                _logger.LogInformation("Tarea {TareaId} deleted successfully", id);
                TempData["SuccessMessage"] = "Tarea eliminada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException while deleting tarea {TareaId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting tarea {TareaId}", id);
                TempData["ErrorMessage"] = "Error al eliminar la tarea: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }
    }
}
