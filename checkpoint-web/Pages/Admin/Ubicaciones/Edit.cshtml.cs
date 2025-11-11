using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Ubicaciones
{
 [Authorize(Roles = "Administrador")]
 public class EditModel : PageModel
 {
 private readonly IUbicacionService _ubicacionService;
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 public EditModel(IUbicacionService ubicacionService, ISedeService sedeService, IAuditService auditService) => (_ubicacionService, _sedeService, _auditService) = (ubicacionService, sedeService, auditService);

 [BindProperty]
 public Ubicacion Ubicacion { get; set; } = new Ubicacion();
 public SelectList Sedes { get; set; } = new SelectList(new List<Sede>(), "Id", "Nombre");

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null) return NotFound();
 var ubicacion = await _ubicacionService.GetByIdAsync(id.Value);
 if (ubicacion == null) return NotFound();
 Ubicacion = ubicacion;
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre", Ubicacion.SedeId);
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre", Ubicacion.SedeId);
 return Page();
 }
 var before = await _ubicacionService.GetByIdAsync(Ubicacion.Id);
 await _ubicacionService.UpdateAsync(Ubicacion);
 // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"UpdateUbicacion:{Ubicacion.Id}", System.Text.Json.JsonSerializer.Serialize(new { Before = before, After = Ubicacion }));
 return RedirectToPage("Index");
 }
 }
}
