using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Sedes
{
 [Authorize(Roles = "Administrador")]
 public class CreateModel : PageModel
 {
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 public CreateModel(ISedeService sedeService, IAuditService auditService) => (_sedeService, _auditService) = (sedeService, auditService);

 [BindProperty]
 public Sede Sede { get; set; } = new Sede();

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 await _sedeService.CreateAsync(Sede);
 // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"CreateSede:{Sede.Codigo}", System.Text.Json.JsonSerializer.Serialize(Sede));
 return RedirectToPage("Index");
 }
 }
}
