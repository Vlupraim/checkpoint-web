using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace checkpoint_web.Services
{
 public class UbicacionService : IUbicacionService
 {
 private readonly CheckpointDbContext _context;
 public UbicacionService(CheckpointDbContext context) => _context = context;

 public async Task<IList<Ubicacion>> GetAllAsync()
 {
 return await _context.Ubicaciones.Include(u => u.Sede).AsNoTracking().ToListAsync();
 }

 public async Task<IList<Ubicacion>> GetBySedeAsync(Guid sedeId)
 {
 return await _context.Ubicaciones.Where(u => u.SedeId == sedeId).AsNoTracking().ToListAsync();
 }

 public async Task<Ubicacion?> GetByIdAsync(Guid id)
 {
 return await _context.Ubicaciones.Include(u => u.Sede).FirstOrDefaultAsync(u => u.Id == id);
 }

 public async Task CreateAsync(Ubicacion ubicacion)
 {
 ubicacion.Id = Guid.NewGuid();
 _context.Ubicaciones.Add(ubicacion);
 await _context.SaveChangesAsync();
 }

 public async Task UpdateAsync(Ubicacion ubicacion)
 {
 var existing = await _context.Ubicaciones.FindAsync(ubicacion.Id);
 if (existing == null) throw new InvalidOperationException("Ubicación no encontrada");
 existing.SedeId = ubicacion.SedeId;
 existing.Codigo = ubicacion.Codigo;
 existing.Nombre = ubicacion.Nombre;
 existing.Tipo = ubicacion.Tipo;
 existing.Capacidad = ubicacion.Capacidad;
 await _context.SaveChangesAsync();
 }

 public async Task DeleteAsync(Guid id)
 {
 var existing = await _context.Ubicaciones.FindAsync(id);
 if (existing == null) throw new InvalidOperationException("Ubicación no encontrada");

 // Check for dependent movimientos that would block deletion (FK restrict)
 var movimientosCount = await _context.Movimientos
 .CountAsync(m => (m.OrigenUbicacionId != null && m.OrigenUbicacionId == id)
 || (m.DestinoUbicacionId != null && m.DestinoUbicacionId == id));

 if (movimientosCount > 0)
 {
 throw new InvalidOperationException("No se puede eliminar la ubicación: existen movimientos asociados. Elimine o reasigne esos registros primero.");
 }

 _context.Ubicaciones.Remove(existing);
 await _context.SaveChangesAsync();
 }
 }
}
