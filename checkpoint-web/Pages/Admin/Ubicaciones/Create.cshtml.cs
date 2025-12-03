using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
 catch (DbUpdateException dbEx)
 {
 // Capturar el error específico de la base de datos
 var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
 
 // Detectar errores comunes de PostgreSQL
 if (innerMessage.Contains("duplicate key") || innerMessage.Contains("23505"))
 {
 TempData["ErrorMessage"] = $"Ya existe una ubicación con el código '{Ubicacion.Codigo}' en esta sede.";
 }
 else if (innerMessage.Contains("foreign key") || innerMessage.Contains("23503"))
 {
 TempData["ErrorMessage"] = "Error de integridad referencial. Verifique que la sede seleccionada existe.";
 }
 else
 {
 TempData["ErrorMessage"] = $"Error de base de datos: {innerMessage}";
 }
 
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 return Page();
 }
 catch (Exception ex)
 {
 // Capturar cualquier otro error
 var detailedMessage = ex.InnerException?.Message ?? ex.Message;
 TempData["ErrorMessage"] = $"Error al crear la ubicación: {detailedMessage}";
 
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 return Page();
 }
 }
 }
}
