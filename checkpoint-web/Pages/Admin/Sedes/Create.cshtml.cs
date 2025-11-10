using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

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
 await _auditService.LogAsync(User.Identity?.Name ?? "anonymous", $"CreateSede:{Sede.Codigo}", System.Text.Json.JsonSerializer.Serialize(Sede));
 return RedirectToPage("Index");
 }
 }
}
