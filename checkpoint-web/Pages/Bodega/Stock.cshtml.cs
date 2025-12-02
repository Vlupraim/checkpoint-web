using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
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

        public async Task OnGetAsync()
        {
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
