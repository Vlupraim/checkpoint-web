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
        public SelectList UbicacionesDisponibles { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");
        public IList<Movimiento> MovimientosHoy { get; set; } = new List<Movimiento>();

        [BindProperty]
        public string Tipo { get; set; } = string.Empty;

        [BindProperty]
        public Guid LoteId { get; set; }

        [BindProperty]
        public Guid UbicacionId { get; set; }

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
            // Validaciones
            if (string.IsNullOrWhiteSpace(Tipo))
            {
                ModelState.AddModelError(nameof(Tipo), "Debe seleccionar un tipo de movimiento");
            }

            if (LoteId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(LoteId), "Debe seleccionar un lote");
            }

            if (UbicacionId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(UbicacionId), "Debe seleccionar una ubicación");
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
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

                // Crear movimiento según el tipo
                switch (Tipo.ToLower())
                {
                    case "entrada":
                    case "ingreso":
                        await _movimientoService.CrearIngresoAsync(LoteId, UbicacionId, Cantidad, usuarioId);
                        TempData["SuccessMessage"] = $"Entrada registrada correctamente: {Cantidad} unidades";
                        break;

                    case "salida":
                        await _movimientoService.CrearSalidaAsync(LoteId, UbicacionId, Cantidad, null, usuarioId, Observaciones);
                        TempData["SuccessMessage"] = $"Salida registrada correctamente: {Cantidad} unidades";
                        break;

                    case "ajuste":
                        if (string.IsNullOrWhiteSpace(Observaciones))
                        {
                            ModelState.AddModelError(nameof(Observaciones), "Los ajustes requieren observaciones");
                            await CargarDatosAsync();
                            return Page();
                        }
                        await _movimientoService.CrearAjusteAsync(LoteId, UbicacionId, Cantidad, usuarioId, Observaciones);
                        TempData["SuccessMessage"] = "Ajuste registrado correctamente (pendiente aprobación)";
                        break;

                    default:
                        TempData["ErrorMessage"] = "Tipo de movimiento no válido";
                        await CargarDatosAsync();
                        return Page();
                }

                _logger.LogInformation("[MOVIMIENTOS] Movimiento {Tipo} creado - Lote: {LoteId}, Cantidad: {Cantidad}, Usuario: {UsuarioId}", 
                    Tipo, LoteId, Cantidad, usuarioId);

                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[MOVIMIENTOS] Operación inválida al crear movimiento {Tipo}", Tipo);
                TempData["ErrorMessage"] = ex.Message;
                await CargarDatosAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOVIMIENTOS] Error al crear movimiento {Tipo}", Tipo);
                TempData["ErrorMessage"] = $"Error al registrar el movimiento: {ex.Message}";
                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                // Cargar lotes liberados con stock disponible
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

                // Cargar ubicaciones activas
                var ubicaciones = await _context.Ubicaciones
                    .Include(u => u.Sede)
                    .OrderBy(u => u.Sede!.Nombre).ThenBy(u => u.Codigo)
                    .AsNoTracking()
                    .ToListAsync();

                UbicacionesDisponibles = new SelectList(
                    ubicaciones.Select(u => new
                    {
                        u.Id,
                        Display = $"{u.Sede?.Nombre ?? "Sin sede"} - {u.Codigo} ({u.Tipo})"
                    }),
                    "Id",
                    "Display"
                );

                // Cargar movimientos de hoy
                var hoy = DateTime.UtcNow.Date;
                MovimientosHoy = await _context.Movimientos
                    .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                    .Include(m => m.OrigenUbicacion)
                    .Include(m => m.DestinoUbicacion)
                    .Where(m => m.Fecha >= hoy && m.Fecha < hoy.AddDays(1))
                    .OrderByDescending(m => m.Fecha)
                    .Take(50)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[MOVIMIENTOS] Datos cargados - {LotesCount} lotes, {UbicacionesCount} ubicaciones, {MovimientosCount} movimientos hoy", 
                    lotesDisponibles.Count, ubicaciones.Count, MovimientosHoy.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOVIMIENTOS] Error al cargar datos");
                TempData["ErrorMessage"] = "Error al cargar datos del sistema";
            }
        }
    }
}
