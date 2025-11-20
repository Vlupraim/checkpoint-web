using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
  public interface IProveedorService
{
  Task<IEnumerable<Proveedor>> GetAllAsync();
        Task<IEnumerable<Proveedor>> GetActivosAsync();
        Task<Proveedor?> GetByIdAsync(int id);
        Task<Proveedor> CreateAsync(Proveedor proveedor);
     Task<Proveedor> UpdateAsync(Proveedor proveedor);
  Task<bool> DeleteAsync(int id);
        Task<Dictionary<string, int>> GetEstadisticasAsync();
    }

    public class ProveedorService : IProveedorService
    {
        private readonly CheckpointDbContext _context;
   private readonly IAuditService _auditService;

 public ProveedorService(CheckpointDbContext context, IAuditService auditService)
 {
         _context = context;
      _auditService = auditService;
}

public async Task<IEnumerable<Proveedor>> GetAllAsync() =>
   await _context.Proveedores.OrderBy(p => p.Nombre).ToListAsync();

        public async Task<IEnumerable<Proveedor>> GetActivosAsync() =>
        await _context.Proveedores.Where(p => p.Activo && p.Estado == "Activo").OrderBy(p => p.Nombre).ToListAsync();

        public async Task<Proveedor?> GetByIdAsync(int id) =>
      await _context.Proveedores.FindAsync(id);

        public async Task<Proveedor> CreateAsync(Proveedor proveedor)
     {
      proveedor.FechaRegistro = DateTime.Now;
  _context.Proveedores.Add(proveedor);
  await _context.SaveChangesAsync();
     await _auditService.LogAsync("admin", $"Creó proveedor: {proveedor.Nombre} (ID: {proveedor.Id})");
       return proveedor;
     }

        public async Task<Proveedor> UpdateAsync(Proveedor proveedor)
 {
      proveedor.UltimaActualizacion = DateTime.Now;
_context.Entry(proveedor).State = EntityState.Modified;
         await _context.SaveChangesAsync();
        await _auditService.LogAsync("admin", $"Actualizó proveedor: {proveedor.Nombre} (ID: {proveedor.Id})");
     return proveedor;
   }

        public async Task<bool> DeleteAsync(int id)
        {
       var proveedor = await _context.Proveedores.FindAsync(id);
if (proveedor == null) return false;
 proveedor.Activo = false;
   await _context.SaveChangesAsync();
 await _auditService.LogAsync("admin", $"Eliminó proveedor: {proveedor.Nombre} (ID: {id})");
  return true;
        }

        public async Task<Dictionary<string, int>> GetEstadisticasAsync()
        {
    return new Dictionary<string, int>
     {
         ["Total"] = await _context.Proveedores.CountAsync(p => p.Activo),
      ["Activos"] = await _context.Proveedores.CountAsync(p => p.Activo && p.Estado == "Activo"),
      ["Homologados"] = await _context.Proveedores.CountAsync(p => p.Activo && p.Estado == "Homologado")
      };
  }
    }
}
