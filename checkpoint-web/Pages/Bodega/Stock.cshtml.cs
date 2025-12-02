using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task OnGetAsync()
        {
            // Cargar Sedes y Ubicaciones para los filtros
            var sedes = await _context.Sedes.AsNoTracking().ToListAsync();
            Sedes = new SelectList(sedes, "Id", "Nombre");
            
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
                "Codigo"
            );

            // Consultar todos los stocks con navegación completa
            Stocks = await _context.Stocks
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Producto)
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Proveedor)
                .Include(s => s.Ubicacion)
                    .ThenInclude(u => u!.Sede)
                .Where(s => s.Cantidad > 0) // Solo stocks con cantidad > 0
                .OrderBy(s => s.Lote!.Producto!.Nombre)
                .ThenBy(s => s.Ubicacion!.Codigo)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
