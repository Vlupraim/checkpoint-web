using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Data;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Ajustes
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;
        private readonly IAuditService _auditService;

        public IndexModel(CheckpointDbContext context, IMovimientoService movimientoService, IAuditService auditService)
        {
            _context = context;
            _movimientoService = movimientoService;
            _auditService = auditService;
        }

        public IList<Movimiento> AjustesPendientes { get; set; } = new List<Movimiento>();
        public IList<Movimiento> AjustesAprobados { get; set; } = new List<Movimiento>();
        public IList<Movimiento> AjustesRechazados { get; set; } = new List<Movimiento>();

        public async Task OnGetAsync()
        {
            await CargarAjustesAsync();
        }

        public async Task<IActionResult> OnPostAprobarAsync(Guid id)
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                var resultado = await _movimientoService.AprobarAjusteAsync(id, usuarioId);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "? Ajuste aprobado y stock actualizado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "? No se pudo aprobar el ajuste";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = $"? {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"? Error al aprobar ajuste: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRechazarAsync(Guid id, string? motivoRechazo)
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                
                var movimiento = await _context.Movimientos
                    .Include(m => m.Lote)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movimiento == null || movimiento.Tipo != "Ajuste")
                {
                    TempData["ErrorMessage"] = "? Ajuste no encontrado";
                    return RedirectToPage();
                }

                // Rechazar ajuste
                movimiento.Estado = "Rechazado";
                movimiento.AprobadoPor = usuarioId;
                movimiento.FechaAprobacion = DateTime.UtcNow;
                movimiento.Motivo += $" | [RECHAZADO por {usuarioId}] {motivoRechazo}";

                // Devolver lote a su estado anterior (Liberado)
                movimiento.Lote!.Estado = EstadoLote.Liberado;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(usuarioId,
                    $"Rechazó ajuste ID: {id}",
                    $"Motivo: {motivoRechazo}");

                TempData["SuccessMessage"] = "? Ajuste rechazado. El lote ha sido devuelto a estado Liberado.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"? Error al rechazar ajuste: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task CargarAjustesAsync()
        {
            var todosAjustes = await _context.Movimientos
                .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                .Include(m => m.OrigenUbicacion).ThenInclude(u => u!.Sede)
                .Include(m => m.DestinoUbicacion).ThenInclude(u => u!.Sede)
                .Where(m => m.Tipo == "Ajuste")
                .OrderByDescending(m => m.Fecha)
                .AsNoTracking()
                .ToListAsync();

            AjustesPendientes = todosAjustes.Where(m => m.Estado == "Pendiente").ToList();
            AjustesAprobados = todosAjustes.Where(m => m.Estado == "Aprobado").Take(20).ToList();
            AjustesRechazados = todosAjustes.Where(m => m.Estado == "Rechazado").Take(20).ToList();
        }
    }
}
