using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega,ControlCalidad")]
    public class LotesModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        public LotesModel(CheckpointDbContext context)
        {
            _context = context;
        }

        public IList<Lote> Lotes { get; set; } = new List<Lote>();
        public int CountCuarentena { get; set; }
        public int CountLiberado { get; set; }
        public int CountRechazado { get; set; }
        public int CountBloqueado { get; set; }

        public async Task OnGetAsync()
        {
            // Load recent lotes with related Producto and Sede via Stocks->Ubicacion->Sede not necessary for listing
            Lotes = await _context.Lotes
                .Include(l => l.Producto)
                .OrderByDescending(l => l.FechaIngreso)
                .Take(100)
                .AsNoTracking()
                .ToListAsync();

            CountCuarentena = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Cuarentena);
            CountLiberado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Liberado);
            CountRechazado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Rechazado);
            CountBloqueado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Bloqueado);
        }
    }
}
