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
   FechaCreacion = DateTime.Now,
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
if (notificacion != null)
   {
 notificacion.Leida = true;
     notificacion.FechaLeida = DateTime.Now;
    await _context.SaveChangesAsync();
 }
}

  public async Task MarcarTodasComoLeidasAsync(string usuarioId)
{
     var notificaciones = await _context.Notificaciones
    .Where(n => n.UsuarioId == usuarioId && !n.Leida)
   .ToListAsync();

   foreach (var n in notificaciones)
 {
   n.Leida = true;
      n.FechaLeida = DateTime.Now;
 }

      await _context.SaveChangesAsync();
        }

 public async Task GenerarNotificacionesAutomaticasAsync()
 {
  // Tareas próximas a vencer (2 días)
    var tareasProximasVencer = await _context.Tareas
      .Where(t => t.Activo 
       && t.Estado != "Finalizada" 
      && t.Estado != "Cancelada"
       && t.FechaLimite.HasValue 
    && t.FechaLimite.Value <= DateTime.Now.AddDays(2)
    && t.FechaLimite.Value > DateTime.Now)
      .ToListAsync();

  foreach (var tarea in tareasProximasVencer)
 {
      if (!string.IsNullOrEmpty(tarea.ResponsableId))
    {
 // Verificar si ya existe notificación reciente
     var existeNotificacion = await _context.Notificaciones
   .AnyAsync(n => n.UsuarioId == tarea.ResponsableId
        && n.ReferenciaId == tarea.Id.ToString()
      && n.FechaCreacion >= DateTime.Now.AddHours(-12));

      if (!existeNotificacion)
 {
      await CrearNotificacionAsync(
  tarea.ResponsableId,
   "Alerta",
  $"? Tarea próxima a vencer: {tarea.Titulo}",
    $"Vence el {tarea.FechaLimite:dd/MM/yyyy}",
  "/Admin/Tareas"
  );
  }
      }
     }

    // Lotes próximos a vencer (7 días)
   var lotesProximosVencer = await _context.Lotes
   .Where(l => l.FechaVencimiento.HasValue 
    && l.FechaVencimiento.Value <= DateTime.Now.AddDays(7)
   && l.FechaVencimiento.Value > DateTime.Now
     && l.Estado == "Liberado")
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
       await CrearNotificacionAsync(
    adminId,
      "Advertencia",
   $"?? {lotesProximosVencer.Count} lote(s) próximo(s) a vencer",
    "Revise el inventario para tomar acciones",
 "/Bodega/Recepcion"
  );
 }
  }

     // Ajustes pendientes de aprobación
 var ajustesPendientes = await _context.Movimientos
   .CountAsync(m => m.Tipo == "Ajuste" && m.Estado == "Pendiente");

  if (ajustesPendientes > 0)
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
      && n.FechaCreacion >= DateTime.Now.AddHours(-6));

     if (!existeNotificacion)
  {
      await CrearNotificacionAsync(
        adminId,
  "Información",
   $"?? {ajustesPendientes} ajuste(s) pendiente(s) de aprobación",
   "Revise los ajustes de inventario",
 "/Bodega/Ajustes"
   );
  }
   }
 }
        }
    }
}
