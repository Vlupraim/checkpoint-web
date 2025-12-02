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

            // CORREGIDO: Verificar si hay lotes asociados
            var lotesAsociados = await _context.Lotes.CountAsync(l => l.ProductoId == id);
            if (lotesAsociados > 0)
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar el producto '{existing.Nombre}' porque tiene {lotesAsociados} lote(s) asociado(s). " +
                    $"Los productos con historial de lotes no pueden eliminarse por razones de trazabilidad.");
            }

            // Verificar si hay tareas asociadas
            var tareasAsociadas = await _context.Tareas.CountAsync(t => t.ProductoId == id);
            if (tareasAsociadas > 0)
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar el producto '{existing.Nombre}' porque tiene {tareasAsociadas} tarea(s) asociada(s). " +
                    $"Primero debe completar o reasignar las tareas relacionadas.");
            }

            _context.Productos.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}