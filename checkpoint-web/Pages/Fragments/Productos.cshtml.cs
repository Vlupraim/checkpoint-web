using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Services;
using checkpoint_web.Models;
using System.Collections.Generic;

namespace checkpoint_web.Pages.Fragments
{
    [Authorize(Roles = "Administrador,PersonalBodega")]
    public class ProductosModel : PageModel
 {
 private readonly IProductoService _productoService;
 public ProductosModel(IProductoService productoService) => _productoService = productoService;
 public IList<Producto> Productos { get; set; } = new List<Producto>();
 public async Task OnGetAsync()
 {
 Productos = (await _productoService.GetAllAsync()) as IList<Producto> ?? new List<Producto>();
 }
 }
}
