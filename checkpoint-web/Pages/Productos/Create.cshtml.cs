using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace checkpoint_web.Pages.Productos
{
 [Authorize(Roles = "Administrador,PersonalBodega")]
 public class CreateModel : PageModel
 {
 private readonly IProductoService _productoService;
 private readonly IAuditService _auditService;
 public CreateModel(IProductoService productoService, IAuditService auditService)
 {
 _productoService = productoService;
 _auditService = auditService;
 }

 [BindProperty]
 public Producto Producto { get; set; } = new Producto();

 public void OnGet()
 {
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 return Page();
 }

 await _productoService.CreateAsync(Producto);
 var details = JsonSerializer.Serialize(Producto);
 await _auditService.LogAsync(User.Identity?.Name ?? "anonymous", $"CreateProducto:{Producto.Sku}", details);
 return RedirectToPage("Index");
 }
 }
}
