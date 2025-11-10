using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
    private readonly IProveedorService _proveedorService;
     public EditModel(IProveedorService proveedorService) => _proveedorService = proveedorService;

    [BindProperty]
 public Proveedor Proveedor { get; set; } = new();

  public async Task<IActionResult> OnGetAsync(int id)
        {
       var proveedor = await _proveedorService.GetByIdAsync(id);
  if (proveedor == null) return NotFound();
     Proveedor = proveedor;
     return Page();
  }

   public async Task<IActionResult> OnPostAsync()
        {
  if (!ModelState.IsValid) return Page();
  await _proveedorService.UpdateAsync(Proveedor);
TempData["SuccessMessage"] = "Proveedor actualizado";
 return RedirectToPage("./Index");
}
    }
}
