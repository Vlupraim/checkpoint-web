using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
 public class ProductoService : IProductoService
 {
 private readonly CheckpointDbContext _context;
 public ProductoService(CheckpointDbContext context) => _context = context;

 public async Task<IList<Producto>> GetAllAsync()
 {
 return await _context.Productos.AsNoTracking().ToListAsync();
 }

 public async Task<Producto?> GetByIdAsync(Guid id)
 {
 return await _context.Productos.FindAsync(id);
 }

 public async Task CreateAsync(Producto producto)
 {
 producto.Id = Guid.NewGuid();
 _context.Productos.Add(producto);
 await _context.SaveChangesAsync();
 }

 public async Task UpdateAsync(Producto producto)
 {
 var existing = await _context.Productos.FindAsync(producto.Id);
 if (existing == null) throw new InvalidOperationException("Producto no encontrado");
 existing.Sku = producto.Sku;
 existing.Nombre = producto.Nombre;
 existing.Unidad = producto.Unidad;
 existing.VidaUtilDias = producto.VidaUtilDias;
 existing.TempMin = producto.TempMin;
 existing.TempMax = producto.TempMax;
 existing.StockMinimo = producto.StockMinimo;
 existing.Activo = producto.Activo;
 await _context.SaveChangesAsync();
 }

 public async Task DeleteAsync(Guid id)
 {
 var existing = await _context.Productos.FindAsync(id);
 if (existing == null) throw new InvalidOperationException("Producto no encontrado");
 _context.Productos.Remove(existing);
 await _context.SaveChangesAsync();
 }
 }
}