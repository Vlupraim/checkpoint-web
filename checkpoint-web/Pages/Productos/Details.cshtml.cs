using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Productos
{
 [Authorize(Roles = "Administrador,PersonalBodega")]
 public class DetailsModel : PageModel
 {
 private readonly IProductoService _productoService;
 public DetailsModel(IProductoService productoService) => _productoService = productoService;

 public Producto? Producto { get; set; }

 public async Task<IActionResult> OnGetAsync(Guid? id)
 {
 if (id == null) return NotFound();
 Producto = await _productoService.GetByIdAsync(id.Value);
 if (Producto == null) return NotFound();
 return Page();
 }
 }
}
