using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
    private readonly IProveedorService _proveedorService;

public IndexModel(IProveedorService proveedorService) => _proveedorService = proveedorService;

   public IEnumerable<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
public Dictionary<string, int> Estadisticas { get; set; } = new();
        [BindProperty]
 public Proveedor NuevoProveedor { get; set; } = new();

   public async Task OnGetAsync()
      {
       Proveedores = await _proveedorService.GetAllAsync();
      Estadisticas = await _proveedorService.GetEstadisticasAsync();
   }

 public async Task<IActionResult> OnPostAsync()
{
      if (!ModelState.IsValid)
      {
    Proveedores = await _proveedorService.GetAllAsync();
     Estadisticas = await _proveedorService.GetEstadisticasAsync();
  return Page();
   }
      await _proveedorService.CreateAsync(NuevoProveedor);
TempData["SuccessMessage"] = "Proveedor creado exitosamente";
       return RedirectToPage();
 }

  public async Task<IActionResult> OnPostDeleteAsync(int id)
{
 await _proveedorService.DeleteAsync(id);
   TempData["SuccessMessage"] = "Proveedor eliminado exitosamente";
 return RedirectToPage();
 }
    }
}
