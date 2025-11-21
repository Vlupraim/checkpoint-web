using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Bodega
{
    // CORREGIDO: Solo PersonalBodega puede recepcionar (Admin solo ve)
    [Authorize(Roles = "PersonalBodega")]
    public class RecepcionModel : PageModel
    {
        private readonly IProductoService _productoService;
        private readonly IUbicacionService _ubicacionService;
        private readonly CheckpointDbContext _context;
        private readonly IAuditService _auditService;
        public RecepcionModel(IProductoService productoService, IUbicacionService ubicacionService, CheckpointDbContext context, IAuditService auditService)
        {
            (_productoService, _ubicacionService, _context, _auditService) = (productoService, ubicacionService, context, auditService);
        }

        [BindProperty]
        public Guid ProductoId { get; set; }
        [BindProperty]
        public string CodigoLote { get; set; } = string.Empty;
        [BindProperty]
        public DateTime FechaIngreso { get; set; } = DateTime.Today;
        [BindProperty]
        public DateTime? FechaVencimiento { get; set; } = DateTime.Today.AddDays(180);
        [BindProperty]
        public Guid UbicacionId { get; set; }
        [BindProperty]
        public decimal Cantidad { get; set; }
        [BindProperty]
        public decimal TempIngreso { get; set; }

        public SelectList Productos { get; set; } = new SelectList(new List<Producto>(), "Id", "Nombre");
        public SelectList Ubicaciones { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");

        public async Task OnGetAsync()
        {
            var productos = await _productoService.GetAllAsync();
            Productos = new SelectList(productos, "Id", "Nombre");
            var ubicaciones = await _ubicacionService.GetAllAsync();
            Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Server-side validations for required selects / values
            if (ProductoId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(ProductoId), "Seleccione un producto.");
            }
            if (UbicacionId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(UbicacionId), "Seleccione una ubicación.");
            }
            if (Cantidad <= 0)
            {
                ModelState.AddModelError(nameof(Cantidad), "La cantidad debe ser mayor que 0.");
            }
            if (string.IsNullOrWhiteSpace(CodigoLote))
            {
                ModelState.AddModelError(nameof(CodigoLote), "El código de lote es requerido.");
            }

            if (!ModelState.IsValid)
            {
                var productos = await _productoService.GetAllAsync();
                Productos = new SelectList(productos, "Id", "Nombre");
                var ubicaciones = await _ubicacion_service_fallback();
                Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
                return Page();
            }

            // Verify referenced entities exist to avoid FK violations
            var productoExists = await _context.Productos.FindAsync(ProductoId) != null;
            var ubicacionExists = await _context.Ubicaciones.FindAsync(UbicacionId) != null;
            if (!productoExists)
            {
                ModelState.AddModelError(nameof(ProductoId), "Producto no encontrado.");
            }
            if (!ubicacionExists)
            {
                ModelState.AddModelError(nameof(UbicacionId), "Ubicación no encontrada.");
            }
            if (!ModelState.IsValid)
            {
                var productos = await _producto_service_fallback();
                Productos = new SelectList(productos, "Id", "Nombre");
                var ubicaciones = await _ubicacion_service_fallback();
                Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
                return Page();
            }

            // Convert dates to UTC (Postgres timestamptz requires UTC DateTime.Kind)
            var fechaIngresoUtc = DateTime.SpecifyKind(FechaIngreso.Date, DateTimeKind.Utc);
            DateTime? fechaVencimientoUtc = null;
            if (FechaVencimiento.HasValue)
            {
                fechaVencimientoUtc = DateTime.SpecifyKind(FechaVencimiento.Value.Date, DateTimeKind.Utc);
            }

            // CRÍTICO: Create Lote con estado inicial CUARENTENA
            // El lote NO puede usarse hasta que Calidad lo libere
            var lote = new Lote
            {
                Id = Guid.NewGuid(),
                ProductoId = ProductoId,
                CodigoLote = CodigoLote,
                FechaIngreso = fechaIngresoUtc,
                FechaVencimiento = fechaVencimientoUtc,
                TempIngreso = TempIngreso,
                CantidadInicial = Cantidad,
                CantidadDisponible = Cantidad,
                Estado = EstadoLote.Cuarentena  // ESTADO INICIAL: Cuarentena (esperando revisión de Calidad)
            };
            _context.Lotes.Add(lote);

            // Create initial Stock
            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                LoteId = lote.Id,
                UbicacionId = UbicacionId,
                Cantidad = Cantidad,
                ActualizadoEn = DateTime.UtcNow
            };
            _context.Stocks.Add(stock);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log full exception via audit (if possible) or set error for UI
                var userIdForLog = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                try {
                    await _auditService.LogAsync(userIdForLog, "ErrorRecepcion", ex.ToString());
                } catch {}
                TempData["Error"] = "Error al registrar la recepción. Contacte al administrador.";
                var productos = await _producto_service_fallback();
                Productos = new SelectList(productos, "Id", "Nombre");
                var ubicaciones = await _ubicacion_service_fallback();
                Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
                return Page();
            }

            // Use NameIdentifier claim for user id (consistent with other services)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            // Fallback to system if empty (AuditService ignores anonymous/system if configured)
            if (string.IsNullOrEmpty(userId)) userId = "system";

            await _audit_service_safe(userId,
                $"Recepcion: Lote={CodigoLote}, Cantidad={Cantidad}, Estado={EstadoLote.Cuarentena}",
                System.Text.Json.JsonSerializer.Serialize(new { lote, stock }));

            TempData["Message"] = $"Recepción registrada correctamente. Lote en CUARENTENA esperando revisión de Calidad.";
            // Redirect to Lotes list so the user sees the created lote immediately
            return RedirectToPage("/Bodega/Lotes");
        }

        // helper to load ubicaciones safely (in case service injection had casing issue earlier)
        private async Task<IEnumerable<Ubicacion>> _ubicacion_service_fallback()
        {
            try
            {
                return await _ubicacionService.GetAllAsync();
            }
            catch
            {
                // fallback to context if service fails
                return await Task.FromResult(_context.Ubicaciones.Include(u => u.Sede).AsNoTracking().ToList());
            }
        }

        private async Task<IEnumerable<Producto>> _producto_service_fallback()
        {
            try
            {
                return await _productoService.GetAllAsync();
            }
            catch
            {
                return await Task.FromResult(_context.Productos.AsNoTracking().ToList());
            }
        }

        private async Task _audit_service_safe(string userId, string action, string details)
        {
            try
            {
                await _auditService.LogAsync(userId, action, details);
            }
            catch
            {
                // swallow to avoid failing the request
            }
        }
    }
}
