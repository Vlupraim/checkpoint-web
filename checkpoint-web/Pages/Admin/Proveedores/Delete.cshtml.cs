using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly IProveedorService _proveedorService;
        private readonly IAuditService _auditService;
        private readonly CheckpointDbContext _context;

        public DeleteModel(IProveedorService proveedorService, IAuditService auditService, CheckpointDbContext context)
        {
            _proveedorService = proveedorService;
            _auditService = auditService;
            _context = context;
        }

        [BindProperty]
        public Proveedor Proveedor { get; set; } = new();
        
        public int LotesAsociados { get; set; }

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
            LotesAsociados = await _context.Lotes.CountAsync(l => l.ProveedorId == id.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de proveedor no válido";
                return RedirectToPage("./Index");
            }

            try
            {
                var proveedor = await _proveedorService.GetByIdAsync(id.Value);
                var result = await _proveedorService.DeleteAsync(id.Value);
                
                if (!result)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el proveedor";
                    return RedirectToPage("./Index");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                await _auditService.LogAsync(userId, $"Eliminó proveedor: {proveedor?.Nombre} (ID: {id})", string.Empty);
                
                TempData["SuccessMessage"] = "Proveedor eliminado correctamente";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el proveedor: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }
    }
}
