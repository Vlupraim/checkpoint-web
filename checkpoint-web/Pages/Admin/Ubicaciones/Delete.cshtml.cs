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
 private readonly IAuditService _auditService;
 public DeleteModel(IUbicacionService ubicacionService, IAuditService auditService) => (_ubicacionService, _auditService) = (ubicacionService, auditService);

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
 var before = await _ubicacionService.GetByIdAsync(id.Value);
 await _ubicacionService.DeleteAsync(id.Value);
 // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"DeleteUbicacion:{id}", System.Text.Json.JsonSerializer.Serialize(before));
 return RedirectToPage("Index");
 }
 }
}
