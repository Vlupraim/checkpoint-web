using Microsoft.AspNetCore.Mvc.RazorPages;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.AspNetCore.Authorization;

namespace checkpoint_web.Pages.Productos
{
 [Authorize(Roles = "Administrador,PersonalBodega")]
 public class IndexModel : PageModel
 {
 private readonly IProductoService _productoService;
 public IndexModel(IProductoService productoService)
 {
 _productoService = productoService;
 }

 public IList<Producto> Productos { get; set; } = new List<Producto>();

 public async Task OnGetAsync()
 {
 Productos = (await _productoService.GetAllAsync()) as IList<Producto> ?? new List<Producto>();
 }
 }
}
