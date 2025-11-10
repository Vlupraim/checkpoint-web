using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Models;

namespace checkpoint_web.Services
{
 public interface IUbicacionService
 {
 Task<IList<Ubicacion>> GetAllAsync();
 Task<IList<Ubicacion>> GetBySedeAsync(Guid sedeId);
 Task<Ubicacion?> GetByIdAsync(Guid id);
 Task CreateAsync(Ubicacion ubicacion);
 Task UpdateAsync(Ubicacion ubicacion);
 Task DeleteAsync(Guid id);
 }
}
