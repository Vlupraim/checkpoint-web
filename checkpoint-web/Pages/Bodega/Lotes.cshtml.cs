using Microsoft.AspNetCore.Mvc;
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

        [BindProperty(SupportsGet = true)]
        public string? CodigoLote { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Producto { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Estado { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Vencimiento { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Base query con navegación completa
                var query = _context.Lotes
                    .Include(l => l.Producto)
                    .Include(l => l.Proveedor)
                    .AsQueryable();

                // Aplicar filtro de código de lote
                if (!string.IsNullOrWhiteSpace(CodigoLote))
                {
                    query = query.Where(l => l.CodigoLote.Contains(CodigoLote));
                }

                // Aplicar filtro de producto (nombre o SKU)
                if (!string.IsNullOrWhiteSpace(Producto))
                {
                    query = query.Where(l => 
                        l.Producto!.Nombre.Contains(Producto) ||
                        l.Producto!.Sku.Contains(Producto)
                    );
                }

                // Aplicar filtro de estado
                if (!string.IsNullOrWhiteSpace(Estado))
                {
                    if (Enum.TryParse<EstadoLote>(Estado, out var estadoEnum))
                    {
                        query = query.Where(l => l.Estado == estadoEnum);
                    }
                }

                // Aplicar filtro de vencimiento
                if (!string.IsNullOrWhiteSpace(Vencimiento))
                {
                    var hoy = DateTime.UtcNow.Date;
                    
                    if (Vencimiento == "proximo")
                    {
                        // Próximos a vencer en 7 días
                        var fechaLimite = hoy.AddDays(7);
                        query = query.Where(l => 
                            l.FechaVencimiento.HasValue &&
                            l.FechaVencimiento.Value >= hoy &&
                            l.FechaVencimiento.Value <= fechaLimite
                        );
                    }
                    else if (Vencimiento == "vencido")
                    {
                        // Ya vencidos
                        query = query.Where(l => 
                            l.FechaVencimiento.HasValue &&
                            l.FechaVencimiento.Value < hoy
                        );
                    }
                }

                // Cargar lotes ordenados por fecha de ingreso (más recientes primero)
                Lotes = await query
                    .OrderByDescending(l => l.FechaIngreso)
                    .Take(100)
                    .AsNoTracking()
                    .ToListAsync();

                // Cargar estadísticas de estados
                CountCuarentena = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Cuarentena);
                CountLiberado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Liberado);
                CountRechazado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Rechazado);
                CountBloqueado = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Bloqueado);
                
                _logger.LogInformation("[LOTES] Loaded {count} lotes with filters", Lotes.Count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading lotes with filters");
                Lotes = new List<Lote>();
                CountCuarentena = CountLiberado = CountRechazado = CountBloqueado = 0;
            }
        }
    }
}
