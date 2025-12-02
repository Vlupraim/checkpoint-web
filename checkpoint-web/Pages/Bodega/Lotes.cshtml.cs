using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega,ControlCalidad")]
    public class LotesModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly ILogger<LotesModel> _logger;
        public LotesModel(CheckpointDbContext context, ILogger<LotesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Lote> Lotes { get; set; } = new List<Lote>();
        public int CountCuarentena { get; set; }
        public int CountLiberado { get; set; }
        public int CountRechazado { get; set; }
        public int CountBloqueado { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load recent lotes with FULL navigation: Producto AND Proveedor
                Lotes = await _context.Lotes
                    .Include(l => l.Producto)      // ? Include Product
                    .Include(l => l.Proveedor)     // ? Include Supplier
                    .OrderByDescending(l => l.FechaIngreso)
                    .Take(100)
                    .AsNoTracking()
                    .ToListAsync();

                CountCuarentena = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Cuarentena);
                CountLiberado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Liberado);
                CountRechazado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Rechazado);
                CountBloqueado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Bloqueado);
                
                _logger.LogInformation("[LOTES] Loaded {count} lotes with navigation", Lotes.Count);
            }
            catch (System.Exception ex)
            {
                // Log and provide safe defaults so view renders without throwing
                _logger.LogError(ex, "Error loading lotes for Lotes page");
                Lotes = new List<Lote>();
                CountCuarentena = CountLiberado = CountRechazado = CountBloqueado = 0;
                // Do not rethrow to avoid HTTP 500 when called via AJAX
            }
        }
    }
}
