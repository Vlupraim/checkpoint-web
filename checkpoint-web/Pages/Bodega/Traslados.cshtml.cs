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
    public class TrasladosModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;
        private readonly ILogger<TrasladosModel> _logger;

        public TrasladosModel(CheckpointDbContext context, IMovimientoService movimientoService, ILogger<TrasladosModel> logger)
        {
            _context = context;
            _movimientoService = movimientoService;
            _logger = logger;
        }

        public SelectList LotesDisponibles { get; set; } = new SelectList(new List<Lote>(), "Id", "CodigoLote");
        public SelectList UbicacionesOrigen { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");
        public SelectList UbicacionesDestino { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");
        public IList<Movimiento> TrasladosRecientes { get; set; } = new List<Movimiento>();

        [BindProperty]
        public Guid LoteId { get; set; }

        [BindProperty]
        public Guid OrigenId { get; set; }

        [BindProperty]
        public Guid DestinoId { get; set; }

        [BindProperty]
        public string? Motivo { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        // API endpoint para obtener stock por lote y ubicación
        public async Task<IActionResult> OnGetStockPorLoteYUbicacionAsync(Guid loteId, Guid ubicacionId)
        {
            try
            {
                var stock = await _context.Stocks
                    .Include(s => s.Ubicacion).ThenInclude(u => u!.Sede)
                    .Include(s => s.Lote).ThenInclude(l => l!.Producto)
                    .FirstOrDefaultAsync(s => s.LoteId == loteId && s.UbicacionId == ubicacionId);

                if (stock != null && stock.Cantidad > 0)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        cantidad = stock.Cantidad,
                        unidad = stock.Unidad,
                        ubicacionCodigo = stock.Ubicacion?.Codigo,
                        productoNombre = stock.Lote?.Producto?.Nombre
                    });
                }

                return new JsonResult(new { success = false, message = "No hay stock en esta ubicación" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TRASLADOS] Error al obtener stock");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validaciones manuales
            if (LoteId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(LoteId), "Debe seleccionar un lote");
            }

            if (OrigenId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(OrigenId), "Debe seleccionar una ubicación de origen");
            }

            if (DestinoId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(DestinoId), "Debe seleccionar una ubicación de destino");
            }

            if (OrigenId != Guid.Empty && DestinoId != Guid.Empty && OrigenId == DestinoId)
            {
                ModelState.AddModelError(string.Empty, "La ubicación de origen y destino no pueden ser la misma");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor complete todos los campos requeridos";
                await CargarDatosAsync();
                return Page();
            }

            try
            {
                // VALIDACIÓN: Asegurar que usuarioId es un GUID válido
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(usuarioId))
                {
                    TempData["ErrorMessage"] = "? Error: No se pudo identificar al usuario. Por favor, inicie sesión nuevamente.";
                    await CargarDatosAsync();
                    return Page();
                }

                // Validar que es un GUID y no un email
                if (!Guid.TryParse(usuarioId, out _))
                {
                    _logger.LogError("[TRASLADOS] UsuarioId no es un GUID válido: {UserId}", usuarioId);
                    TempData["ErrorMessage"] = "? Error de autenticación. El identificador de usuario no es válido.";
                    await CargarDatosAsync();
                    return Page();
                }

                // OBTENER CANTIDAD AUTOMÁTICAMENTE desde el stock de origen
                var cantidadDisponible = await _movimientoService.GetStockDisponibleEnUbicacionAsync(LoteId, OrigenId);
                
                if (cantidadDisponible <= 0)
                {
                    var ubicacion = await _context.Ubicaciones.FindAsync(OrigenId);
                    TempData["ErrorMessage"] = $"? No hay stock disponible en {ubicacion?.Codigo ?? "la ubicación de origen"}. " +
                        $"Seleccione una ubicación que contenga stock de este lote.";
                    await CargarDatosAsync();
                    return Page();
                }
                
                _logger.LogInformation("[TRASLADOS] Trasladando TODO el stock - Usuario: {UserId}, Lote: {LoteId}, Cantidad: {Cantidad}", 
                    usuarioId, LoteId, cantidadDisponible);
                
                await _movimientoService.CrearTrasladoAsync(
                    LoteId,
                    OrigenId,
                    DestinoId,
                    cantidadDisponible, // Usar toda la cantidad disponible
                    usuarioId,
                    Motivo
                );

                var lote = await _context.Lotes.Include(l => l.Producto).FirstOrDefaultAsync(l => l.Id == LoteId);
                TempData["SuccessMessage"] = $"? Traslado ejecutado correctamente: {cantidadDisponible:N2} {lote?.Producto?.Unidad ?? "u"}";
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[TRASLADOS] Operación inválida");
                
                // Mejorar mensaje si es error de stock
                if (ex.Message.Contains("Stock insuficiente"))
                {
                    var ubicacion = await _context.Ubicaciones.FindAsync(OrigenId);
                    var lote = await _context.Lotes.Include(l => l.Producto).FirstOrDefaultAsync(l => l.Id == LoteId);
                    
                    TempData["ErrorMessage"] = $"? No hay stock disponible en {ubicacion?.Codigo ?? "ubicación de origen"}. " +
                        $"Seleccione una ubicación que contenga stock de este lote.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"? {ex.Message}";
                }
                
                await CargarDatosAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TRASLADOS] Error al crear traslado");
                TempData["ErrorMessage"] = $"? Error al registrar el traslado: {ex.Message}";
                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                _logger.LogInformation("[TRASLADOS] Iniciando carga de datos");

                // Cargar lotes LIBERADOS
                var lotesDisponibles = await _context.Lotes
                    .Include(l => l.Producto)
                    .Where(l => l.Estado == EstadoLote.Liberado && l.CantidadDisponible > 0)
                    .OrderByDescending(l => l.FechaIngreso)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[TRASLADOS] Lotes encontrados: {Count}", lotesDisponibles.Count);

                if (lotesDisponibles.Count == 0)
                {
                    _logger.LogWarning("[TRASLADOS] No hay lotes liberados con stock disponible");
                    TempData["ErrorMessage"] = "?? No hay lotes liberados disponibles. Los lotes deben ser aprobados por Control de Calidad antes de poder usarlos.";
                }

                LotesDisponibles = new SelectList(
                    lotesDisponibles.Select(l => new
                    {
                        l.Id,
                        Display = $"{l.CodigoLote} - {l.Producto?.Nombre} ({l.CantidadDisponible:N2})"
                    }),
                    "Id",
                    "Display"
                );

                // Cargar ubicaciones
                var ubicaciones = await _context.Ubicaciones
                    .Include(u => u.Sede)
                    .OrderBy(u => u.Sede!.Nombre)
                    .ThenBy(u => u.Codigo)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[TRASLADOS] Ubicaciones encontradas: {Count}", ubicaciones.Count);

                var ubicacionesItems = ubicaciones.Select(u => new
                {
                    u.Id,
                    Display = $"{u.Sede?.Nombre} - {u.Codigo}"
                }).ToList();

                UbicacionesOrigen = new SelectList(ubicacionesItems, "Id", "Display");
                UbicacionesDestino = new SelectList(ubicacionesItems, "Id", "Display");

                // Cargar traslados recientes
                TrasladosRecientes = await _context.Movimientos
                    .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                    .Include(m => m.OrigenUbicacion).ThenInclude(u => u!.Sede)
                    .Include(m => m.DestinoUbicacion).ThenInclude(u => u!.Sede)
                    .Where(m => m.Tipo == "Traslado")
                    .OrderByDescending(m => m.Fecha)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("[TRASLADOS] Traslados recientes: {Count}", TrasladosRecientes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TRASLADOS] Error al cargar datos");
                TempData["ErrorMessage"] = "Error al cargar datos: " + ex.Message;
            }
        }
    }
}
