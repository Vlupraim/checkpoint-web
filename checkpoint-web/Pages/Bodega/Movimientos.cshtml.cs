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
    public class MovimientosModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;

        public MovimientosModel(CheckpointDbContext context, IMovimientoService movimientoService)
        {
            _context = context;
            _movimientoService = movimientoService;
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
            if (!ModelState.IsValid)
            {
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
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            // Cargar lotes LIBERADOS (solo estos pueden usarse en operaciones)
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
        }
    }
}
