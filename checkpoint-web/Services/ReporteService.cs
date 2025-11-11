using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
    public interface IReporteService
    {
 Task<InventarioReporte> GetInventarioActualAsync();
     Task<IEnumerable<Movimiento>> GetMovimientosAsync(DateTime? desde = null, DateTime? hasta = null, string? tipo = null);
        Task<IEnumerable<Tarea>> GetTareasAsync(string? estado = null, DateTime? desde = null, DateTime? hasta = null);
  Task<ResumenOperativo> GetResumenOperativoAsync(DateTime? fecha = null);
    }

 public class ReporteService : IReporteService
 {
     private readonly CheckpointDbContext _context;

  public ReporteService(CheckpointDbContext context)
        {
  _context = context;
        }

     public async Task<InventarioReporte> GetInventarioActualAsync()
        {
  var stocks = await _context.Stocks
      .Include(s => s.Lote).ThenInclude(l => l!.Producto)
  .Include(s => s.Ubicacion).ThenInclude(u => u!.Sede)
     .Where(s => s.Cantidad > 0)
     .ToListAsync();

       var stocksPorProducto = stocks
 .GroupBy(s => new { s.Lote!.Producto!.Id, s.Lote.Producto.Nombre })
  .Select(g => new StockProducto
        {
 ProductoId = g.Key.Id,
  ProductoNombre = g.Key.Nombre,
   CantidadTotal = g.Sum(s => s.Cantidad),
   Ubicaciones = g.Count(),
       Lotes = g.Select(s => s.LoteId).Distinct().Count()
})
   .ToList();

var stocksPorUbicacion = stocks
      .GroupBy(s => new { s.Ubicacion!.Id, s.Ubicacion.Codigo, s.Ubicacion.Sede!.Nombre })
     .Select(g => new StockUbicacion
  {
 UbicacionId = g.Key.Id,
  UbicacionCodigo = g.Key.Codigo,
SedeNombre = g.Key.Nombre,
 Items = g.Count(),
      CantidadTotal = g.Sum(s => s.Cantidad)
  })
    .ToList();

      return new InventarioReporte
     {
  FechaGeneracion = DateTime.UtcNow, // CORREGIDO: Usar UTC
    TotalProductos = stocksPorProducto.Count,
   TotalUbicaciones = stocksPorUbicacion.Count,
CantidadTotalItems = (int)stocks.Sum(s => s.Cantidad),
   StocksPorProducto = stocksPorProducto,
   StocksPorUbicacion = stocksPorUbicacion
  };
        }

     public async Task<IEnumerable<Movimiento>> GetMovimientosAsync(DateTime? desde = null, DateTime? hasta = null, string? tipo = null)
{
     var query = _context.Movimientos
.Include(m => m.Lote).ThenInclude(l => l!.Producto)
     .Include(m => m.OrigenUbicacion)
.Include(m => m.DestinoUbicacion)
  .Include(m => m.Cliente)
       .AsQueryable();

   if (desde.HasValue)
      query = query.Where(m => m.Fecha >= desde.Value);

       if (hasta.HasValue)
  query = query.Where(m => m.Fecha <= hasta.Value);

   if (!string.IsNullOrEmpty(tipo))
     query = query.Where(m => m.Tipo == tipo);

   return await query.OrderByDescending(m => m.Fecha).ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetTareasAsync(string? estado = null, DateTime? desde = null, DateTime? hasta = null)
   {
     var query = _context.Tareas
 .Include(t => t.Producto)
      .Include(t => t.Lote)
    .Where(t => t.Activo)
   .AsQueryable();

   if (!string.IsNullOrEmpty(estado))
      query = query.Where(t => t.Estado == estado);

  if (desde.HasValue)
     query = query.Where(t => t.FechaCreacion >= desde.Value);

       if (hasta.HasValue)
  query = query.Where(t => t.FechaCreacion <= hasta.Value);

    return await query.OrderByDescending(t => t.FechaCreacion).ToListAsync();
 }

     public async Task<ResumenOperativo> GetResumenOperativoAsync(DateTime? fecha = null)
        {
   // CORREGIDO: Usar UTC
   var fechaBase = fecha ?? DateTime.UtcNow.Date;
 var fechaInicio = fechaBase.Date;
 var fechaFin = fechaInicio.AddDays(1);

return new ResumenOperativo
 {
 Fecha = fechaBase,
  MovimientosDelDia = await _context.Movimientos.CountAsync(m => m.Fecha >= fechaInicio && m.Fecha < fechaFin),
Ingresos = await _context.Movimientos.CountAsync(m => m.Fecha >= fechaInicio && m.Fecha < fechaFin && m.Tipo == "Ingreso"),
       Salidas = await _context.Movimientos.CountAsync(m => m.Fecha >= fechaInicio && m.Fecha < fechaFin && m.Tipo == "Salida"),
 Traslados = await _context.Movimientos.CountAsync(m => m.Fecha >= fechaInicio && m.Fecha < fechaFin && m.Tipo == "Traslado"),
  TareasPendientes = await _context.Tareas.CountAsync(t => t.Activo && t.Estado == "Pendiente"),
       TareasCompletadas = await _context.Tareas.CountAsync(t => t.FechaFinalizacion >= fechaInicio && t.FechaFinalizacion < fechaFin),
LotesPendientesCalidad = await _context.Lotes.CountAsync(l => l.Estado == "Creado" || l.Estado == "PendienteCalidad"),
AjustesPendientes = await _context.Movimientos.CountAsync(m => m.Tipo == "Ajuste" && m.Estado == "Pendiente")
   };
    }
    }

// DTOs
    public class InventarioReporte
    {
 public DateTime FechaGeneracion { get; set; }
   public int TotalProductos { get; set; }
        public int TotalUbicaciones { get; set; }
  public int CantidadTotalItems { get; set; }
 public List<StockProducto> StocksPorProducto { get; set; } = new();
        public List<StockUbicacion> StocksPorUbicacion { get; set; } = new();
    }

   public class StockProducto
    {
    public Guid ProductoId { get; set; }
public string ProductoNombre { get; set; } = string.Empty;
public decimal CantidadTotal { get; set; }
     public int Ubicaciones { get; set; }
  public int Lotes { get; set; }
    }

 public class StockUbicacion
 {
 public Guid UbicacionId { get; set; }
   public string UbicacionCodigo { get; set; } = string.Empty;
     public string SedeNombre { get; set; } = string.Empty;
        public int Items { get; set; }
   public decimal CantidadTotal { get; set; }
    }

  public class ResumenOperativo
    {
public DateTime Fecha { get; set; }
     public int MovimientosDelDia { get; set; }
 public int Ingresos { get; set; }
 public int Salidas { get; set; }
        public int Traslados { get; set; }
  public int TareasPendientes { get; set; }
public int TareasCompletadas { get; set; }
  public int LotesPendientesCalidad { get; set; }
 public int AjustesPendientes { get; set; }
    }
}
