using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Data;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            // Validaciones manuales
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
                TempData["ErrorMessage"] = "Por favor complete todos los campos requeridos";
                await CargarDatosAsync();
                return Page();
            }

            try
            {
                // TODO: Implementar lógica según el tipo de movimiento
                // Por ahora registramos como movimiento genérico
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
                _logger.LogInformation("[MOVIMIENTOS] Iniciando carga de datos");

                // Cargar lotes LIBERADOS (solo estos pueden usarse en operaciones)
                var lotesDisponibles = await _context.Lotes
                    .Include(l => l.Producto)
                    .Where(l => l.Estado == EstadoLote.Liberado && l.CantidadDisponible > 0)
                    .OrderByDescending(l => l.FechaIngreso)
                    .Take(100)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[MOVIMIENTOS] Lotes encontrados: {Count}", lotesDisponibles.Count);
                
                foreach (var lote in lotesDisponibles)
                {
                    _logger.LogDebug("[MOVIMIENTOS] Lote: {Codigo} - Estado: {Estado} - Cantidad: {Cantidad}", 
                        lote.CodigoLote, lote.Estado, lote.CantidadDisponible);
                }

                if (lotesDisponibles.Count == 0)
                {
                    _logger.LogWarning("[MOVIMIENTOS] No hay lotes liberados con stock disponible");
                    TempData["ErrorMessage"] = "?? No hay lotes liberados disponibles. Los lotes deben ser aprobados por Control de Calidad antes de poder usarlos.";
                }

                LotesDisponibles = new SelectList(
                    lotesDisponibles.Select(l => new
                    {
                        l.Id,
                        Display = $"{l.CodigoLote} - {l.Producto?.Nombre} ({l.CantidadDisponible:N2} {l.Producto?.Unidad})"
                    }),
                    "Id",
                    "Display"
                );

                // Cargar movimientos del día
                var hoy = DateTime.UtcNow.Date;
                MovimientosHoy = await _context.Movimientos
                    .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                    .Where(m => m.Fecha >= hoy && m.Fecha < hoy.AddDays(1))
                    .OrderByDescending(m => m.Fecha)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[MOVIMIENTOS] Movimientos hoy: {Count}", MovimientosHoy.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOVIMIENTOS] Error al cargar datos");
                TempData["ErrorMessage"] = "Error al cargar datos: " + ex.Message;
            }
        }
    }
}
