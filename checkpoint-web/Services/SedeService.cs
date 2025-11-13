using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
 public class SedeService : ISedeService
 {
 private readonly CheckpointDbContext _context;
 public SedeService(CheckpointDbContext context) => _context = context;

 public async Task<IList<Sede>> GetAllAsync()
 {
 return await _context.Sedes.AsNoTracking().ToListAsync();
 }

 public async Task<Sede?> GetByIdAsync(Guid id)
 {
 return await _context.Sedes.FindAsync(id);
 }

 public async Task CreateAsync(Sede sede)
 {
 sede.Id = Guid.NewGuid();
 _context.Sedes.Add(sede);
 await _context.SaveChangesAsync();
 }

 public async Task UpdateAsync(Sede sede)
 {
 var existing = await _context.Sedes.FindAsync(sede.Id);
 if (existing == null) throw new InvalidOperationException("Sede no encontrada");
 existing.Nombre = sede.Nombre;
 existing.Codigo = sede.Codigo;
 existing.Direccion = sede.Direccion;
 existing.Activa = sede.Activa;
 await _context.SaveChangesAsync();
 }

 public async Task DeleteAsync(Guid id)
 {
 var existing = await _context.Sedes.FindAsync(id);
 if (existing == null) throw new InvalidOperationException("Sede no encontrada");

 // Check dependent ubicaciones and related movimientos/stocks to avoid FK violations
 var ubicacionIds = await _context.Ubicaciones
 .Where(u => u.SedeId == id)
 .Select(u => u.Id)
 .ToListAsync();

 if (ubicacionIds.Any())
 {
 var movimientosCount = await _context.Movimientos
 .CountAsync(m => (m.OrigenUbicacionId != null && ubicacionIds.Contains(m.OrigenUbicacionId.Value))
 || (m.DestinoUbicacionId != null && ubicacionIds.Contains(m.DestinoUbicacionId.Value)));

 var stocksCount = await _context.Stocks
 .CountAsync(s => s.UbicacionId != null && ubicacionIds.Contains(s.UbicacionId));

 if (movimientosCount >0 || stocksCount >0)
 {
 throw new InvalidOperationException("No se puede eliminar la sede: existen movimientos o stock asociados a sus ubicaciones. Elimine o reasigne esos registros antes de eliminar la sede.");
 }
 }

 // Safe to remove (no blocking dependents)
 _context.Sedes.Remove(existing);
 await _context.SaveChangesAsync();
 }
 }
}
