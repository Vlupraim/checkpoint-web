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
 public class CreateModel : PageModel
 {
 private readonly IUbicacionService _ubicacionService;
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 public CreateModel(IUbicacionService ubicacionService, ISedeService sedeService, IAuditService auditService) => (_ubicacionService, _sedeService, _auditService) = (ubicacionService, sedeService, auditService);

 [BindProperty]
 public Ubicacion Ubicacion { get; set; } = new Ubicacion();
 public SelectList Sedes { get; set; } = new SelectList(new List<Sede>(), "Id", "Nombre");

 public async Task OnGetAsync()
 {
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 }

 public async Task<IActionResult> OnPostAsync()
 {
 // Validate that a sede was selected (prevent Guid.Empty being saved)
 if (Ubicacion.SedeId == Guid.Empty)
 {
 ModelState.AddModelError("Ubicacion.SedeId", "Debe seleccionar una sede.");
 }

 if (!ModelState.IsValid)
 {
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 return Page();
 }

 try
 {
 await _ubicacionService.CreateAsync(Ubicacion);
 // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"CreateUbicacion:{Ubicacion.Codigo}", System.Text.Json.JsonSerializer.Serialize(Ubicacion));
 TempData["SuccessMessage"] = "Ubicación creada correctamente";
 return RedirectToPage("Index");
 }
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = "Error al crear la ubicación: " + ex.Message;
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 return Page();
 }
 }
 }
}
