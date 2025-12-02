using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Data;
using checkpoint_web.Models;
using checkpoint_web.Services;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega")]
    public class TrasladosModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;

        public TrasladosModel(CheckpointDbContext context, IMovimientoService movimientoService)
        {
            _context = context;
            _movimientoService = movimientoService;
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
        public decimal Cantidad { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            try
            {
                var usuarioId = User.Identity?.Name ?? "unknown";
                
                await _movimientoService.CrearTrasladoAsync(
                    LoteId,
                    OrigenId,
                    DestinoId,
                    Cantidad,
                    usuarioId
                );

                TempData["SuccessMessage"] = "Traslado ejecutado correctamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            // Cargar lotes LIBERADOS
            var lotesDisponibles = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l => l.Estado == EstadoLote.Liberado && l.CantidadDisponible > 0)
                .OrderByDescending(l => l.FechaIngreso)
                .AsNoTracking()
                .ToListAsync();

            LotesDisponibles = new SelectList(
                lotesDisponibles.Select(l => new
                {
                    l.Id,
                    Display = $"{l.CodigoLote} - {l.Producto?.Nombre}"
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
        }
    }
}
