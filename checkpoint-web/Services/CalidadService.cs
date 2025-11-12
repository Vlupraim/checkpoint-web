using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
    public interface ICalidadService
    {
        Task<IEnumerable<Lote>> GetLotesPendientesRevisionAsync();
 Task<IEnumerable<CalidadLiberacion>> GetHistorialCalidadAsync(int limit = 50);
      Task<CalidadLiberacion?> GetByIdAsync(Guid id);
        Task<CalidadLiberacion> CrearRevisionAsync(Guid loteId, string usuarioId);
Task<CalidadLiberacion> AprobarLoteAsync(Guid loteId, string usuarioId, string? observacion = null);
   Task<CalidadLiberacion> RechazarLoteAsync(Guid loteId, string usuarioId, string motivo);
  Task<CalidadLiberacion> BloquearLoteAsync(Guid loteId, string usuarioId, string motivo);
Task<Dictionary<string, int>> GetEstadisticasAsync();
  }

    public class CalidadService : ICalidadService
 {
   private readonly CheckpointDbContext _context;
 private readonly IAuditService _auditService;

        public CalidadService(CheckpointDbContext context, IAuditService auditService)
     {
 _context = context;
     _auditService = auditService;
}

     public async Task<IEnumerable<Lote>> GetLotesPendientesRevisionAsync()
    {
     // Solo lotes en Cuarentena están pendientes de revisión
   return await _context.Lotes
   .Include(l => l.Producto)
.Include(l => l.Proveedor)
    .Where(l => l.Estado == EstadoLote.Cuarentena)
 .OrderBy(l => l.FechaIngreso)
     .ToListAsync();
   }

public async Task<IEnumerable<CalidadLiberacion>> GetHistorialCalidadAsync(int limit = 50)
   {
   return await _context.CalidadLiberaciones
    .Include(c => c.Lote).ThenInclude(l => l!.Producto)
.OrderByDescending(c => c.Fecha)
    .Take(limit)
     .ToListAsync();
        }

public async Task<CalidadLiberacion?> GetByIdAsync(Guid id)
     {
   return await _context.CalidadLiberaciones
     .Include(c => c.Lote).ThenInclude(l => l!.Producto)
    .FirstOrDefaultAsync(c => c.Id == id);
     }

        public async Task<CalidadLiberacion> CrearRevisionAsync(Guid loteId, string usuarioId)
 {
       var lote = await _context.Lotes.FindAsync(loteId);
 if (lote == null)
     throw new KeyNotFoundException("Lote no encontrado");

 var revision = new CalidadLiberacion
 {
    Id = Guid.NewGuid(),
       LoteId = loteId,
 UsuarioId = usuarioId,
          Fecha = DateTime.UtcNow,
 Estado = "EnRevision",
      Observacion = "Revisión iniciada"
        };

       lote.Estado = EstadoLote.EnRevision;
    _context.CalidadLiberaciones.Add(revision);
      await _context.SaveChangesAsync();

    await _auditService.LogAsync(usuarioId, $"Inició revisión de calidad para lote {lote.CodigoLote}");
return revision;
  }

   public async Task<CalidadLiberacion> AprobarLoteAsync(Guid loteId, string usuarioId, string? observacion = null)
     {
 var lote = await _context.Lotes.FindAsync(loteId);
    if (lote == null)
   throw new KeyNotFoundException("Lote no encontrado");

            var liberacion = new CalidadLiberacion
{
      Id = Guid.NewGuid(),
LoteId = loteId,
UsuarioId = usuarioId,
    Fecha = DateTime.UtcNow,
     Estado = "Aprobado",
   Observacion = observacion ?? "Lote aprobado - Cumple especificaciones"
 };

 lote.Estado = EstadoLote.Liberado;
  _context.CalidadLiberaciones.Add(liberacion);
await _context.SaveChangesAsync();

await _auditService.LogAsync(usuarioId, $"Aprobó lote {lote.CodigoLote} para uso");
return liberacion;
        }

   public async Task<CalidadLiberacion> RechazarLoteAsync(Guid loteId, string usuarioId, string motivo)
 {
       var lote = await _context.Lotes.FindAsync(loteId);
  if (lote == null)
throw new KeyNotFoundException("Lote no encontrado");

            var rechazo = new CalidadLiberacion
     {
        Id = Guid.NewGuid(),
    LoteId = loteId,
 UsuarioId = usuarioId,
     Fecha = DateTime.UtcNow,
  Estado = "Rechazado",
       Observacion = motivo
 };

          lote.Estado = EstadoLote.Rechazado;
   _context.CalidadLiberaciones.Add(rechazo);
       await _context.SaveChangesAsync();

        await _auditService.LogAsync(usuarioId, $"Rechazó lote {lote.CodigoLote}: {motivo}");
       return rechazo;
     }

        public async Task<CalidadLiberacion> BloquearLoteAsync(Guid loteId, string usuarioId, string motivo)
{
        var lote = await _context.Lotes.FindAsync(loteId);
if (lote == null)
    throw new KeyNotFoundException("Lote no encontrado");

       var bloqueo = new CalidadLiberacion
{
   Id = Guid.NewGuid(),
LoteId = loteId,
    UsuarioId = usuarioId,
 Fecha = DateTime.UtcNow,
 Estado = "Bloqueado",
         Observacion = motivo
   };

    lote.Estado = EstadoLote.Bloqueado;
   _context.CalidadLiberaciones.Add(bloqueo);
       await _context.SaveChangesAsync();

await _auditService.LogAsync(usuarioId, $"Bloqueó lote {lote.CodigoLote}: {motivo}");
            return bloqueo;
     }

  public async Task<Dictionary<string, int>> GetEstadisticasAsync()
        {
   return new Dictionary<string, int>
      {
       ["LotesPendientes"] = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Cuarentena),
     ["LotesAprobados"] = await _context.CalidadLiberaciones.CountAsync(c => c.Estado == "Aprobado"),
 ["LotesRechazados"] = await _context.CalidadLiberaciones.CountAsync(c => c.Estado == "Rechazado"),
   ["LotesBloqueados"] = await _context.Lotes.CountAsync(l => l.Estado == EstadoLote.Bloqueado)
 };
        }
  }
}
