using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Admin.Sedes
{
 [Authorize(Roles = "Administrador")]
 public class DeleteModel : PageModel
 {
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 private readonly ILogger<DeleteModel> _logger;

 public DeleteModel(ISedeService sedeService, IAuditService auditService, ILogger<DeleteModel> logger)
 {
 _sedeService = sedeService;
 _auditService = auditService;
 _logger = logger;
 }

 [BindProperty]
 public Sede Sede { get; set; } = new Sede();

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null)
 {
 _logger.LogWarning("OnGetAsync: id was null");
 return NotFound();
 }

 var sede = await _sedeService.GetByIdAsync(id.Value);
 if (sede == null)
 {
 _logger.LogWarning("OnGetAsync: Sede {SedeId} not found", id);
 return NotFound();
 }

 Sede = sede;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync(Guid? id)
 {
 if (id == null)
 {
 _logger.LogWarning("OnPostAsync: id was null");
 return NotFound();
 }

 try
 {
 var before = await _sedeService.GetByIdAsync(id.Value);
 if (before == null)
 {
 _logger.LogWarning("OnPostAsync: Sede {SedeId} not found", id);
 TempData["ErrorMessage"] = "La sede no fue encontrada.";
 return RedirectToPage("Index");
 }

 _logger.LogInformation("Attempting to delete Sede {SedeId} - {SedeName}", id, before.Nombre);

 await _sedeService.DeleteAsync(id.Value);

 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"DeleteSede:{id}", System.Text.Json.JsonSerializer.Serialize(before));

 _logger.LogInformation("Sede {SedeId} deleted successfully by {UserId}", id, userId);
 TempData["SuccessMessage"] = "Sede eliminada correctamente";
 return RedirectToPage("Index");
 }
 catch (InvalidOperationException ex)
 {
 _logger.LogWarning(ex, "Cannot delete Sede {SedeId}: {Message}", id, ex.Message);
 TempData["ErrorMessage"] = ex.Message;
 return RedirectToPage("Index");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Unexpected error deleting Sede {SedeId}", id);
 TempData["ErrorMessage"] = "Error al eliminar la sede: " + ex.Message;
 return RedirectToPage("Index");
 }
 }
 }
}
