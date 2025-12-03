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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de proveedor no válido";
                return RedirectToPage("./Index");
            }

            var proveedor = await _proveedorService.GetByIdAsync(id.Value);
            
            if (proveedor == null)
            {
                TempData["ErrorMessage"] = "Proveedor no encontrado";
                return RedirectToPage("./Index");
            }
            
            Proveedor = proveedor;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                return Page();
            }

            try
            {
                await _proveedorService.UpdateAsync(Proveedor);
                TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el proveedor: " + ex.Message;
                return Page();
            }
        }
    }
}
