using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega,ControlCalidad")]
    public class StockModel : PageModel
    {
        private readonly CheckpointDbContext _context;

        public StockModel(CheckpointDbContext context)
        {
            _context = context;
        }

        public IList<Stock> Stocks { get; set; } = new List<Stock>();
        public SelectList Sedes { get; set; } = new SelectList(new List<Sede>(), "Id", "Nombre");
        public SelectList Ubicaciones { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");

        [BindProperty(SupportsGet = true)]
        public string? Producto { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? SedeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? UbicacionId { get; set; }

        public async Task OnGetAsync()
        {
            // Cargar Sedes y Ubicaciones para los filtros
            var sedes = await _context.Sedes
                .Where(s => s.Activa)
                .AsNoTracking()
                .ToListAsync();
            Sedes = new SelectList(sedes, "Id", "Nombre", SedeId);
            
            var ubicaciones = await _context.Ubicaciones
                .Include(u => u.Sede)
                .AsNoTracking()
                .ToListAsync();
            Ubicaciones = new SelectList(
                ubicaciones.Select(u => new { 
                    u.Id, 
                    Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" 
                }), 
                "Id", 
                "Codigo",
                UbicacionId
            );

            // Consultar stocks con filtros aplicados
            var query = _context.Stocks
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Producto)
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Proveedor)
                .Include(s => s.Ubicacion)
                    .ThenInclude(u => u!.Sede)
                .Where(s => s.Cantidad > 0)
                .AsQueryable();

            // Aplicar filtro de producto (nombre o SKU) - CASE INSENSITIVE para PostgreSQL
            if (!string.IsNullOrWhiteSpace(Producto))
            {
                var productoLower = Producto.ToLower();
                query = query.Where(s => 
                    EF.Functions.ILike(s.Lote!.Producto!.Nombre, $"%{Producto}%") ||
                    EF.Functions.ILike(s.Lote!.Producto!.Sku, $"%{Producto}%")
                );
            }

            // Aplicar filtro de sede
            if (SedeId.HasValue)
            {
                query = query.Where(s => s.Ubicacion!.SedeId == SedeId.Value);
            }

            // Aplicar filtro de ubicación
            if (UbicacionId.HasValue)
            {
                query = query.Where(s => s.UbicacionId == UbicacionId.Value);
            }

            Stocks = await query
                .OrderBy(s => s.Lote!.Producto!.Nombre)
                .ThenBy(s => s.Ubicacion!.Codigo)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
