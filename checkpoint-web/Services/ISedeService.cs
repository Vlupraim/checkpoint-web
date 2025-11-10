using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using checkpoint_web.Models;

namespace checkpoint_web.Services
{
 public interface ISedeService
 {
 Task<IList<Sede>> GetAllAsync();
 Task<Sede?> GetByIdAsync(Guid id);
 Task CreateAsync(Sede sede);
 Task UpdateAsync(Sede sede);
 Task DeleteAsync(Guid id);
 }
}
