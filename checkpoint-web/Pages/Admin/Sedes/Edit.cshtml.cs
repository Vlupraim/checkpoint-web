using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Sedes
{
 [Authorize(Roles = "Administrador")]
 public class EditModel : PageModel
 {
 private readonly ISedeService _sedeService;
 private readonly IAuditService _auditService;
 public EditModel(ISedeService sedeService, IAuditService auditService) => (_sedeService, _auditService) = (sedeService, auditService);

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

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 var before = await _sedeService.GetByIdAsync(Sede.Id);
 await _sedeService.UpdateAsync(Sede);
 await _auditService.LogAsync(User.Identity?.Name ?? "anonymous", $"UpdateSede:{Sede.Id}", System.Text.Json.JsonSerializer.Serialize(new { Before = before, After = Sede }));
 return RedirectToPage("Index");
 }
 }
}
