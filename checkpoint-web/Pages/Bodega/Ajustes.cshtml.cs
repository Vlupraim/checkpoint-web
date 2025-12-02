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
    public class AjustesModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly IMovimientoService _movimientoService;

        public AjustesModel(CheckpointDbContext context, IMovimientoService movimientoService)
        {
            _context = context;
            _movimientoService = movimientoService;
        }

        public SelectList LotesDisponibles { get; set; } = new SelectList(new List<Lote>(), "Id", "CodigoLote");
        public SelectList UbicacionesDisponibles { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");

        [BindProperty]
        public Guid LoteId { get; set; }

        [BindProperty]
        public Guid UbicacionId { get; set; }

        [BindProperty]
        public string TipoAjuste { get; set; } = string.Empty;

        [BindProperty]
        public decimal Cantidad { get; set; }

        [BindProperty]
        public string Motivo { get; set; } = string.Empty;

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
                
                // Convertir tipo de ajuste a cantidad con signo
                var cantidadConSigno = TipoAjuste == "Resta" ? -Cantidad : Cantidad;

                await _movimientoService.CrearAjusteAsync(
                    LoteId,
                    UbicacionId,
                    cantidadConSigno,
                    usuarioId,
                    Motivo
                );

                TempData["SuccessMessage"] = "Ajuste creado exitosamente. Esperando aprobación de administrador.";
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
            // Cargar solo lotes LIBERADOS
            var lotes = await _context.Lotes
                .Include(l => l.Producto)
                .Where(l => l.Estado == EstadoLote.Liberado)
                .OrderByDescending(l => l.FechaIngreso)
                .AsNoTracking()
                .ToListAsync();

            LotesDisponibles = new SelectList(
                lotes.Select(l => new
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

            UbicacionesDisponibles = new SelectList(
                ubicaciones.Select(u => new
                {
                    u.Id,
                    Display = $"{u.Sede?.Nombre} - {u.Codigo}"
                }),
                "Id",
                "Display"
            );
        }
    }
}
