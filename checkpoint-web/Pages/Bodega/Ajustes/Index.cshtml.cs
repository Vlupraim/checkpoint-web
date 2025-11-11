using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Bodega.Ajustes
{
    [Authorize(Roles = "Administrador,PersonalBodega")]
    public class IndexModel : PageModel
    {
        private readonly IMovimientoService _movimientoService;

        public IndexModel(IMovimientoService movimientoService)
        {
            _movimientoService = movimientoService;
        }

        public IEnumerable<Movimiento> AjustesPendientes { get; set; } = new List<Movimiento>();
        public IEnumerable<Movimiento> AjustesAprobados { get; set; } = new List<Movimiento>();

        public async Task OnGetAsync()
        {
            var ajustes = await _movimientoService.GetByTipoAsync("Ajuste");
            AjustesPendientes = ajustes.Where(a => a.Estado == "Pendiente");
            AjustesAprobados = ajustes.Where(a => a.Estado == "Aprobado").Take(20);
        }

        public async Task<IActionResult> OnPostAprobarAsync(Guid id)
        {
            if (!User.IsInRole("Administrador"))
            {
                TempData["ErrorMessage"] = "Solo administradores pueden aprobar ajustes";
                return RedirectToPage();
            }

            // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity?.Name (email)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            var exito = await _movimientoService.AprobarAjusteAsync(id, userId);
            TempData[exito ? "SuccessMessage" : "ErrorMessage"] =
                exito ? "Ajuste aprobado exitosamente" : "Error al aprobar ajuste";

            return RedirectToPage();
        }
    }
}
