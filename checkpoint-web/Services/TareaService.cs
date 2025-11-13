using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace checkpoint_web.Services
{
    public class TareaService : ITareaService
    {
        private readonly CheckpointDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificacionService _notificacionService;

        public TareaService(CheckpointDbContext context, IAuditService auditService, INotificacionService notificacionService)
        {
            _context = context;
            _auditService = auditService;
            _notificacionService = notificacionService;
        }

        public async Task<IEnumerable<Tarea>> GetAllAsync()
        {
            return await _context.Tareas
          .Include(t => t.Producto)
        .Include(t => t.Lote)
                .Where(t => t.Activo)
       .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetByUsuarioAsync(string usuarioId)
        {
          return await _context.Tareas
        .Include(t => t.Producto)
  .Include(t => t.Lote)
       .Where(t => t.Activo && (t.ResponsableId == usuarioId || t.CreadoPor == usuarioId))
       .OrderByDescending(t => t.FechaCreacion)
         .ToListAsync();
        }

 public async Task<IEnumerable<Tarea>> GetPendientesAsync()
        {
            return await _context.Tareas
    .Include(t => t.Producto)
     .Include(t => t.Lote)
       .Where(t => t.Activo && t.Estado == "Pendiente")
   .OrderBy(t => t.FechaLimite)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetPorEstadoAsync(string estado)
        {
        return await _context.Tareas
         .Include(t => t.Producto)
        .Include(t => t.Lote)
        .Where(t => t.Activo && t.Estado == estado)
                .OrderByDescending(t => t.FechaCreacion)
     .ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetProximasAVencerAsync(int dias = 7)
        {
 var fechaLimite = DateTime.UtcNow.AddDays(dias);
            return await _context.Tareas
         .Include(t => t.Producto)
                .Include(t => t.Lote)
  .Where(t => t.Activo 
           && t.Estado != "Finalizada" 
 && t.Estado != "Cancelada"
   && t.FechaLimite.HasValue 
         && t.FechaLimite.Value <= fechaLimite)
        .OrderBy(t => t.FechaLimite)
      .ToListAsync();
     }

        public async Task<Tarea?> GetByIdAsync(int id)
        {
            return await _context.Tareas
         .Include(t => t.Producto)
     .Include(t => t.Lote)
     .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tarea> CreateAsync(Tarea tarea)
        {
     tarea.FechaCreacion = DateTime.UtcNow;
   tarea.Progreso = 0;
    
        _context.Tareas.Add(tarea);
      await _context.SaveChangesAsync();

    await _auditService.LogAsync(
      tarea.CreadoPor ?? "system",
        $"Creó tarea: {tarea.Titulo} (ID: {tarea.Id})"
   );

            // Enviar notificación al responsable si está asignado
      if (!string.IsNullOrEmpty(tarea.ResponsableId))
     {
     await _notificacionService.CrearNotificacionAsync(
    tarea.ResponsableId,
      "TareaNueva",
   $"?? Nueva tarea asignada: {tarea.Titulo}",
       tarea.Descripcion ?? "Sin descripción",
  $"/Admin/Tareas"
   );
      }

  return tarea;
      }

        public async Task<Tarea> UpdateAsync(Tarea tarea)
     {
     var existing = await _context.Tareas.FindAsync(tarea.Id);
 if (existing == null)
   throw new KeyNotFoundException($"Tarea {tarea.Id} no encontrada");

         // Registrar cambios en historial
  var cambios = new List<string>();
 
          if (existing.Estado != tarea.Estado)
  cambios.Add($"Estado: {existing.Estado} ? {tarea.Estado}");
 
         if (existing.ResponsableId != tarea.ResponsableId)
    {
       cambios.Add($"Responsable cambiado");
    
      // Notificar al nuevo responsable si es diferente
    if (!string.IsNullOrEmpty(tarea.ResponsableId) && tarea.ResponsableId != existing.ResponsableId)
       {
         await _notificacionService.CrearNotificacionAsync(
 tarea.ResponsableId,
     "TareaReasignada",
     $"?? Tarea reasignada: {tarea.Titulo}",
     $"Se te ha asignado esta tarea. Estado: {tarea.Estado}",
         $"/Admin/Tareas"
      );
       }
        }

        if (existing.Progreso != tarea.Progreso)
   cambios.Add($"Progreso: {existing.Progreso}% ? {tarea.Progreso}%");

       if (cambios.Any())
    {
            var historialEntry = new
    {
 Fecha = DateTime.UtcNow,
  Cambios = cambios
     };
      
    var historialList = string.IsNullOrEmpty(existing.Historial)
    ? new List<object>()
         : JsonSerializer.Deserialize<List<object>>(existing.Historial) ?? new List<object>();
       
       historialList.Add(historialEntry);
             tarea.Historial = JsonSerializer.Serialize(historialList);
      }

     _context.Entry(existing).CurrentValues.SetValues(tarea);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
    "system",
$"Actualizó tarea: {tarea.Titulo} (ID: {tarea.Id})"
   );

   return tarea;
    }

        public async Task<bool> DeleteAsync(int id)
        {
var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
  return false;

            tarea.Activo = false;
       await _context.SaveChangesAsync();

            await _auditService.LogAsync(
          "system",
    $"Eliminó tarea: {tarea.Titulo} (ID: {id})"
       );

 return true;
   }

        public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado, string? usuarioId = null)
  {
         var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
       return false;

            var estadoAnterior = tarea.Estado;
            tarea.Estado = nuevoEstado;

       if (nuevoEstado == "EnProgreso" && !tarea.FechaInicio.HasValue)
            {
           tarea.FechaInicio = DateTime.UtcNow;
            }

     if (nuevoEstado == "Finalizada")
         {
      tarea.FechaFinalizacion = DateTime.UtcNow;
     tarea.Progreso = 100;
            }

            await _context.SaveChangesAsync();

        await _auditService.LogAsync(
       usuarioId ?? "system",
           $"Cambió estado de tarea {id}: {estadoAnterior} ? {nuevoEstado}"
  );

      return true;
        }

     public async Task<bool> AsignarResponsableAsync(int id, string responsableId)
        {
   var tarea = await _context.Tareas.FindAsync(id);
         if (tarea == null)
   return false;

     tarea.ResponsableId = responsableId;
    await _context.SaveChangesAsync();

    await _auditService.LogAsync(
 "system",
        $"Asignó responsable a tarea {id}"
            );

            return true;
        }

    public async Task<bool> ActualizarProgresoAsync(int id, int progreso)
    {
        var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
    return false;

         tarea.Progreso = Math.Clamp(progreso, 0, 100);
   
       if (progreso >= 100)
       {
     tarea.Estado = "Finalizada";
              tarea.FechaFinalizacion = DateTime.UtcNow;
 }

      await _context.SaveChangesAsync();
            return true;
     }

        public async Task<Dictionary<string, int>> GetEstadisticasAsync()
        {
       var now = DateTime.UtcNow;
            var stats = new Dictionary<string, int>
     {
     ["Total"] = await _context.Tareas.CountAsync(t => t.Activo),
     ["Pendientes"] = await _context.Tareas.CountAsync(t => t.Activo && t.Estado == "Pendiente"),
    ["EnProgreso"] = await _context.Tareas.CountAsync(t => t.Activo && t.Estado == "EnProgreso"),
      ["Finalizadas"] = await _context.Tareas.CountAsync(t => t.Activo && t.Estado == "Finalizada"),
     ["Vencidas"] = await _context.Tareas.CountAsync(t => t.Activo 
    && t.Estado != "Finalizada" 
 && t.FechaLimite.HasValue 
     && t.FechaLimite.Value < now)
            };

            return stats;
  }
    }
}
