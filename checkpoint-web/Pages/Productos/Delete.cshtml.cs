using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Security.Claims;

namespace checkpoint_web.Pages.Productos
{
 [Authorize(Roles = "Administrador,PersonalBodega")]
 public class DeleteModel : PageModel
 {
 private readonly IProductoService _productoService;
 private readonly IAuditService _auditService;
 public DeleteModel(IProductoService productoService, IAuditService auditService) => (_productoService, _auditService) = (productoService, auditService);

 [BindProperty]
 public Producto Producto { get; set; } = new Producto();

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null)
 {
 TempData["ErrorMessage"] = "ID de producto no válido";
 return RedirectToPage("Index");
 }
 
 var producto = await _productoService.GetByIdAsync(id.Value);
 if (producto == null)
 {
 TempData["ErrorMessage"] = "Producto no encontrado";
 return RedirectToPage("Index");
 }
 
 Producto = producto;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync(Guid? id)
 {
 if (id == null)
 {
 TempData["ErrorMessage"] = "ID de producto no válido";
 return RedirectToPage("Index");
 }

 try
 {
 var before = await _productoService.GetByIdAsync(id.Value);
 await _productoService.DeleteAsync(id.Value);
 
 var details = JsonSerializer.Serialize(before);
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
 await _auditService.LogAsync(userId, $"DeleteProducto:{id}", details);
 
 TempData["SuccessMessage"] = "Producto eliminado correctamente";
 return RedirectToPage("Index");
 }
 catch (InvalidOperationException ex)
 {
 TempData["ErrorMessage"] = ex.Message;
 return RedirectToPage("Index");
 }
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
 return RedirectToPage("Index");
 }
 }
 }
}
