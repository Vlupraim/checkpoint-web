using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace checkpoint_web.Pages.Productos
{
 [Authorize(Roles = "Administrador,PersonalBodega")]
 public class EditModel : PageModel
 {
 private readonly IProductoService _productoService;
 private readonly IAuditService _auditService;
 public EditModel(IProductoService productoService, IAuditService auditService) => (_productoService, _auditService) = (productoService, auditService);

 [BindProperty]
 public Producto Producto { get; set; } = new Producto();

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null) return NotFound();
 var producto = await _productoService.GetByIdAsync(id.Value);
 if (producto == null) return NotFound();
 Producto = producto;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 var before = await _productoService.GetByIdAsync(Producto.Id);
 await _productoService.UpdateAsync(Producto);
 var after = await _productoService.GetByIdAsync(Producto.Id);
 var details = JsonSerializer.Serialize(new { Before = before, After = after });
 await _auditService.LogAsync(User.Identity?.Name ?? "anonymous", $"UpdateProducto:{Producto.Id}", details);
 return RedirectToPage("Index");
 }
 }
}
