using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Data;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega")]
    public class MovimientosModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;
        private readonly ILogger<MovimientosModel> _logger;

        public MovimientosModel(CheckpointDbContext context, IMovimientoService movimientoService, ILogger<MovimientosModel> logger)
        {
            _context = context;
            _movimientoService = movimientoService;
            _logger = logger;
        }

        public SelectList LotesDisponibles { get; set; } = new SelectList(new List<Lote>(), "Id", "CodigoLote");
        public IList<Movimiento> MovimientosHoy { get; set; } = new List<Movimiento>();

        [BindProperty]
        public string Tipo { get; set; } = string.Empty;

        [BindProperty]
        public Guid LoteId { get; set; }

        [BindProperty]
        public decimal Cantidad { get; set; }

        [BindProperty]
        public string? Observaciones { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Tipo))
            {
                ModelState.AddModelError(nameof(Tipo), "Debe seleccionar un tipo de movimiento");
            }

            if (LoteId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(LoteId), "Debe seleccionar un lote");
            }

            if (Cantidad <= 0)
            {
                ModelState.AddModelError(nameof(Cantidad), "La cantidad debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            try
            {
                TempData["SuccessMessage"] = "Movimiento registrado correctamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOVIMIENTOS] Error al crear movimiento");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                var lotesDisponibles = await _context.Lotes
                    .Include(l => l.Producto)
                    .Where(l => l.Estado == EstadoLote.Liberado && l.CantidadDisponible > 0)
                    .OrderByDescending(l => l.FechaIngreso)
                    .Take(100)
                    .AsNoTracking()
                    .ToListAsync();

                LotesDisponibles = new SelectList(
                    lotesDisponibles.Select(l => new
                    {
                        l.Id,
                        Display = $"{l.CodigoLote} - {l.Producto?.Nombre ?? "Sin nombre"} ({l.CantidadDisponible:N2} {l.Producto?.Unidad ?? "u"})"
                    }),
                    "Id",
                    "Display"
                );

                var hoy = DateTime.UtcNow.Date;
                MovimientosHoy = await _context.Movimientos
                    .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                    .Where(m => m.Fecha >= hoy && m.Fecha < hoy.AddDays(1))
                    .OrderByDescending(m => m.Fecha)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOVIMIENTOS] Error al cargar datos");
                TempData["ErrorMessage"] = "Error al cargar datos del sistema";
            }
        }
    }
}
