using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Sedes
{
 [Authorize(Roles = "Administrador")]
 public class DeleteModel : PageModel
 {
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 public DeleteModel(ISedeService sedeService, IAuditService auditService) => (_sedeService, _auditService) = (sedeService, auditService);

 [BindProperty]
 public Sede Sede { get; set; } = new Sede();

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null) return NotFound();
 var sede = await _sedeService.GetByIdAsync(id.Value);
 if (sede == null) return NotFound();
 Sede = sede;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync(Guid? id)
 {
 if (id == null) return NotFound();
 try
 {
 var before = await _sedeService.GetByIdAsync(id.Value);
 await _sedeService.DeleteAsync(id.Value);
 // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"DeleteSede:{id}", System.Text.Json.JsonSerializer.Serialize(before));
 TempData["SuccessMessage"] = "Sede eliminada correctamente";
 return RedirectToPage("Index");
 }
 catch (InvalidOperationException ex)
 {
 TempData["ErrorMessage"] = ex.Message;
 return RedirectToPage("Index");
 }
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = "Error al eliminar la sede: " + ex.Message;
 return RedirectToPage("Index");
 }
 }
 }
}
