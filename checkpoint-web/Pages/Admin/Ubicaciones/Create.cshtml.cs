using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;

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
 if (!ModelState.IsValid)
 {
 var sedes = await _sedeService.GetAllAsync();
 Sedes = new SelectList(sedes, "Id", "Nombre");
 return Page();
 }
 await _ubicacionService.CreateAsync(Ubicacion);
 await _auditService.LogAsync(User.Identity?.Name ?? "anonymous", $"CreateUbicacion:{Ubicacion.Codigo}", System.Text.Json.JsonSerializer.Serialize(Ubicacion));
 return RedirectToPage("Index");
 }
 }
}
