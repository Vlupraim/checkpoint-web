using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
    public interface INotificacionService
    {
  Task<IEnumerable<Notificacion>> GetNotificacionesUsuarioAsync(string usuarioId, bool soloNoLeidas = false);
   Task<int> GetCountNoLeidasAsync(string usuarioId);
        Task<Notificacion> CrearNotificacionAsync(string usuarioId, string tipo, string titulo, string? mensaje = null, string? url = null);
     Task MarcarComoLeidaAsync(int notificacionId);
   Task MarcarTodasComoLeidasAsync(string usuarioId);
        Task GenerarNotificacionesAutomaticasAsync();
   Task EliminarNotificacionAsync(int notificacionId, string usuarioId);
    }

    public class NotificacionService : INotificacionService
    {
     private readonly CheckpointDbContext _context;

   public NotificacionService(CheckpointDbContext context)
        {
     _context = context;
 }

     public async Task<IEnumerable<Notificacion>> GetNotificacionesUsuarioAsync(string usuarioId, bool soloNoLeidas = false)
        {
     var query = _context.Notificaciones
  .Where(n => n.UsuarioId == usuarioId && n.Activa)
    .AsQueryable();

if (soloNoLeidas)
   query = query.Where(n => !n.Leida);

     return await query.OrderByDescending(n => n.FechaCreacion).Take(50).ToListAsync();
 }

   public async Task<int> GetCountNoLeidasAsync(string usuarioId)
{
       return await _context.Notificaciones
  .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida && n.Activa);
  }

 public async Task<Notificacion> CrearNotificacionAsync(string usuarioId, string tipo, string titulo, string? mensaje = null, string? url = null)
{
            var notificacion = new Notificacion
   {
      UsuarioId = usuarioId,
Tipo = tipo,
Titulo = titulo,
Mensaje = mensaje,
     Url = url,
   FechaCreacion = DateTime.UtcNow,
  Leida = false,
      Activa = true,
   Prioridad = tipo == "Alerta" ? "Alta" : "Media"
     };

 _context.Notificaciones.Add(notificacion);
    await _context.SaveChangesAsync();
 return notificacion;
}

public async Task MarcarComoLeidaAsync(int notificacionId)
        {
       var notificacion = await _context.Notificaciones.FindAsync(notificacionId);
if (notificacion != null && notificacion.Activa)
   {
 notificacion.Leida = true;
     notificacion.FechaLeida = DateTime.UtcNow;
    await _context.SaveChangesAsync();
 }
}

  public async Task MarcarTodasComoLeidasAsync(string usuarioId)
{
     var notificaciones = await _context.Notificaciones
    .Where(n => n.UsuarioId == usuarioId && !n.Leida && n.Activa)
   .ToListAsync();

   foreach (var n in notificaciones)
 {
   n.Leida = true;
      n.FechaLeida = DateTime.UtcNow;
 }

      await _context.SaveChangesAsync();
        }

 public async Task EliminarNotificacionAsync(int notificacionId, string usuarioId)
 {
     var n = await _context.Notificaciones.FindAsync(notificacionId);
 if (n == null) return;

     // Solo permitir eliminación a propietarios o admins; aquí se realiza la eliminación lógica si coincide el usuario
 if (n.UsuarioId == usuarioId)
 {
 n.Activa = false;
 await _context.SaveChangesAsync();
 }
 }

 public async Task GenerarNotificacionesAutomaticasAsync()
 {
  var ahora = DateTime.UtcNow;
 var todayStart = ahora.Date;
    var dosDiasDespues = ahora.AddDays(2);
    var sieteDiasDespues = ahora.AddDays(7);
    var doceHorasAtras = ahora.AddHours(-12);
    var seisHorasAtras = ahora.AddHours(-6);
    var unDiaDespues = ahora.AddDays(1);
 
  // Tareas que vencen HOY (urgentes)
       var tareasVencenHoy = await _context.Tareas
   .Where(t => t.Activo 
 && t.Estado != "Finalizada" 
  && t.Estado != "Cancelada"
      && t.FechaLimite.HasValue 
   && t.FechaLimite.Value.Date == ahora.Date)
    .ToListAsync();

    foreach (var tarea in tareasVencenHoy)
      {
  if (!string.IsNullOrEmpty(tarea.ResponsableId))
  {
       // Sólo considerar notificaciones activas al verificar duplicados
    var existeNotificacion = await _context.Notificaciones
     .AnyAsync(n => n.UsuarioId == tarea.ResponsableId
       && n.ReferenciaId == tarea.Id.ToString()
&& n.Tipo == "AlertaVenceHoy"
  && n.Activa
  && n.FechaCreacion >= todayStart);

    if (!existeNotificacion)
       {
 var notif = await CrearNotificacionAsync(
     tarea.ResponsableId,
      "AlertaVenceHoy",
 $"URGENTE: Tarea vence HOY: {tarea.Titulo}",
    $"Esta tarea debe completarse hoy. Prioridad: {tarea.Prioridad}",
     "/Tareas/MisTareas"
     );

 notif.ReferenciaId = tarea.Id.ToString();
 notif.Categoria = "Tareas";
 await _context.SaveChangesAsync();
     }
    }
 }

  // Tareas próximas a vencer (mañana)
       var tareasVencenManana = await _context.Tareas
 .Where(t => t.Activo 
     && t.Estado != "Finalizada" 
  && t.Estado != "Cancelada"
     && t.FechaLimite.HasValue 
 && t.FechaLimite.Value.Date == ahora.AddDays(1).Date)
  .ToListAsync();

    foreach (var tarea in tareasVencenManana)
      {
  if (!string.IsNullOrEmpty(tarea.ResponsableId))
        {
 var existeNotificacion = await _context.Notificaciones
   .AnyAsync(n => n.UsuarioId == tarea.ResponsableId
 && n.ReferenciaId == tarea.Id.ToString()
     && n.Tipo == "AlertaVenceManana"
      && n.Activa
      && n.FechaCreacion >= todayStart);

   if (!existeNotificacion)
       {
   var notif = await CrearNotificacionAsync(
     tarea.ResponsableId,
     "AlertaVenceManana",
     $"Tarea vence MAÑANA: {tarea.Titulo}",
        $"Recuerda completar esta tarea. Progreso actual: {tarea.Progreso}%",
       "/Tareas/MisTareas"
   );

 notif.ReferenciaId = tarea.Id.ToString();
 notif.Categoria = "Tareas";
 await _context.SaveChangesAsync();
       }
    }
        }

  // Tareas próximas a vencer (2 días)
    var tareasProximasVencer = await _context.Tareas
      .Where(t => t.Activo 
  && t.Estado != "Finalizada" 
      && t.Estado != "Cancelada"
       && t.FechaLimite.HasValue 
 && t.FechaLimite.Value <= dosDiasDespues
    && t.FechaLimite.Value > unDiaDespues)
    .ToListAsync();

  foreach (var tarea in tareasProximasVencer)
 {
      if (!string.IsNullOrEmpty(tarea.ResponsableId))
    {
 // Verificar si ya existe notificación activa reciente
  var existeNotificacion = await _context.Notificaciones
   .AnyAsync(n => n.UsuarioId == tarea.ResponsableId
     && n.ReferenciaId == tarea.Id.ToString()
 && n.Activa
 && n.FechaCreacion >= doceHorasAtras);

      if (!existeNotificacion)
 {
      var notif = await CrearNotificacionAsync(
  tarea.ResponsableId,
   "Alerta",
  $"Tarea próxima a vencer: {tarea.Titulo}",
    $"Vence el {tarea.FechaLimite:dd/MM/yyyy}. Progreso: {tarea.Progreso}%",
  "/Tareas/MisTareas"
  );

 notif.ReferenciaId = tarea.Id.ToString();
 notif.Categoria = "Tareas";
 await _context.SaveChangesAsync();
  }
      }
}

    // Tareas VENCIDAS (recordatorio diario)
        var tareasVencidas = await _context.Tareas
       .Where(t => t.Activo 
   && t.Estado != "Finalizada" 
  && t.Estado != "Cancelada"
   && t.FechaLimite.HasValue 
   && t.FechaLimite.Value < ahora)
        .ToListAsync();

    foreach (var tarea in tareasVencidas)
       {
      if (!string.IsNullOrEmpty(tarea.ResponsableId))
      {
         // Solo una notificación por día para tareas vencidas (considerar sólo activas)
   var existeNotificacion = await _context.Notificaciones
  .AnyAsync(n => n.UsuarioId == tarea.ResponsableId
      && n.ReferenciaId == tarea.Id.ToString()
    && n.Tipo == "TareaVencida"
     && n.Activa
     && n.FechaCreacion >= todayStart);

      if (!existeNotificacion)
 {
     var notif = await CrearNotificacionAsync(
 tarea.ResponsableId,
      "TareaVencida",
   $"Tarea VENCIDA: {tarea.Titulo}",
         $"Esta tarea venció el {tarea.FechaLimite:dd/MM/yyyy}. Requiere atención inmediata.",
       "/Tareas/MisTareas"
);

 notif.ReferenciaId = tarea.Id.ToString();
 notif.Categoria = "Tareas";
 await _context.SaveChangesAsync();
 }
   }
  }

    // Lotes próximos a vencer (7 días)
   var lotesProximosVencer = await _context.Lotes
   .Where(l => l.FechaVencimiento.HasValue 
&& l.FechaVencimiento.Value <= sieteDiasDespues
   && l.FechaVencimiento.Value > ahora
     && l.Estado == EstadoLote.Liberado)
    .ToListAsync();

    if (lotesProximosVencer.Any())
  {
      // Notificar a todos los administradores
     var admins = await _context.UserRoles
     .Where(ur => ur.RoleId == (
  _context.Roles.First(r => r.Name == "Administrador").Id
 ))
    .Select(ur => ur.UserId)
    .ToListAsync();

      foreach (var adminId in admins)
{
 var notif = await CrearNotificacionAsync(
    adminId,
    "Advertencia",
$"{lotesProximosVencer.Count} lote(s) próximo(s) a vencer",
    "Revise el inventario para tomar acciones",
 "/Bodega/Recepcion"
  );

 notif.Categoria = "Inventario";
 await _context.SaveChangesAsync();
 }
  }

     // Ajustes pendientes de aprobación
 var ajustesPendientes = await _context.Movimientos
   .CountAsync(m => m.Tipo == "Ajuste" && m.Estado == "Pendiente");

  if (ajustesPendientes >0)
  {
 var admins = await _context.UserRoles
 .Where(ur => ur.RoleId == (
 _context.Roles.First(r => r.Name == "Administrador").Id
 ))
 .Select(ur => ur.UserId)
 .ToListAsync();

 foreach (var adminId in admins)
 {
 var existeNotificacion = await _context.Notificaciones
 .AnyAsync(n => n.UsuarioId == adminId
 && n.Categoria == "Inventario"
 && n.Activa
 && n.FechaCreacion >= seisHorasAtras);

 if (!existeNotificacion)
 {
 var notif = await CrearNotificacionAsync(
 adminId,
 "Informacion",
 $"{ajustesPendientes} ajuste(s) pendiente(s) de aprobación",
 "Revise los ajustes de inventario",
 "/Bodega/Ajustes"
 );

 notif.Categoria = "Inventario";
 await _context.SaveChangesAsync();
 }
 }
 }
     }
    }
}
