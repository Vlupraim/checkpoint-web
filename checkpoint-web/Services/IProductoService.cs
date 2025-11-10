using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Models;

namespace checkpoint_web.Services
{
 public interface IProductoService
 {
 Task<IList<Producto>> GetAllAsync();
 Task<Producto?> GetByIdAsync(Guid id);
 Task CreateAsync(Producto producto);
 Task UpdateAsync(Producto producto);
 Task DeleteAsync(Guid id);
 }
}