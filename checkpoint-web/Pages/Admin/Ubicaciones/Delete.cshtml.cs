using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Ubicaciones
{
 [Authorize(Roles = "Administrador")]
 public class DeleteModel : PageModel
 {
 private readonly IUbicacionService _ubicacionService;
 private readonly IAuditService _audit_service;
 public DeleteModel(IUbicacionService ubicacionService, IAuditService auditService) => (_ubicacionService, _audit_service) = (ubicacionService, auditService);

 [BindProperty]
 public Ubicacion Ubicacion { get; set; } = new Ubicacion();

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null) return NotFound();
 var ubicacion = await _ubicacionService.GetByIdAsync(id.Value);
 if (ubicacion == null) return NotFound();
 Ubicacion = ubicacion;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync(Guid? id)
 {
 if (id == null) return NotFound();
 try
 {
 var before = await _ubicacionService.GetByIdAsync(id.Value);
 await _ubicacionService.DeleteAsync(id.Value);
 var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "anonymous";
 await _audit_service.LogAsync(userId, $"DeleteUbicacion:{id}", System.Text.Json.JsonSerializer.Serialize(before));
 TempData["SuccessMessage"] = "Ubicación eliminada correctamente";
 return RedirectToPage("Index");
 }
 catch (InvalidOperationException ex)
 {
 TempData["ErrorMessage"] = ex.Message;
 return RedirectToPage("Index");
 }
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = "Error al eliminar la ubicación: " + ex.Message;
 return RedirectToPage("Index");
 }
 }
 }
}
