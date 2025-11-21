using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<IEnumerable<Cliente>> GetActivosAsync();
        Task<Cliente?> GetByIdAsync(int id);
        Task<Cliente> CreateAsync(Cliente cliente);
   Task<Cliente> UpdateAsync(Cliente cliente);
        Task<bool> DeleteAsync(int id);
        Task<Dictionary<string, int>> GetEstadisticasAsync();
    }

    public class ClienteService : IClienteService
    {
  private readonly CheckpointDbContext _context;
  private readonly IAuditService _auditService;

    public ClienteService(CheckpointDbContext context, IAuditService auditService)
        {
            _context = context;
       _auditService = auditService;
    }

   public async Task<IEnumerable<Cliente>> GetAllAsync() =>
     await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();

      public async Task<IEnumerable<Cliente>> GetActivosAsync() =>
 await _context.Clientes.Where(c => c.Activo && c.Estado == "Activo").OrderBy(c => c.Nombre).ToListAsync();

        public async Task<Cliente?> GetByIdAsync(int id) =>
         await _context.Clientes.FindAsync(id);

        public async Task<Cliente> CreateAsync(Cliente cliente)
        {
            cliente.FechaRegistro = DateTime.Now;
       _context.Clientes.Add(cliente);
await _context.SaveChangesAsync();
       await _auditService.LogAsync("admin", $"Creó cliente: {cliente.Nombre} (ID: {cliente.Id})");
     return cliente;
  }

        public async Task<Cliente> UpdateAsync(Cliente cliente)
        {
          cliente.UltimaActualizacion = DateTime.Now;
            _context.Entry(cliente).State = EntityState.Modified;
         await _context.SaveChangesAsync();
     await _auditService.LogAsync("admin", $"Actualizó cliente: {cliente.Nombre} (ID: {cliente.Id})");
            return cliente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
   var cliente = await _context.Clientes.FindAsync(id);
       if (cliente == null) return false;
            cliente.Activo = false;
 await _context.SaveChangesAsync();
            await _auditService.LogAsync("admin", $"Eliminó cliente: {cliente.Nombre} (ID: {id})");
    return true;
      }

        public async Task<Dictionary<string, int>> GetEstadisticasAsync()
        {
       return new Dictionary<string, int>
            {
       ["Total"] = await _context.Clientes.CountAsync(c => c.Activo),
  ["Activos"] = await _context.Clientes.CountAsync(c => c.Activo && c.Estado == "Activo"),
    ["Inactivos"] = await _context.Clientes.CountAsync(c => c.Activo && c.Estado == "Inactivo")
     };
        }
    }
}
