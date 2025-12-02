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
            cliente.FechaRegistro = DateTime.UtcNow;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("admin", $"Creó cliente: {cliente.Nombre} (ID: {cliente.Id})", string.Empty);
            return cliente;
        }

        public async Task<Cliente> UpdateAsync(Cliente cliente)
        {
            cliente.UltimaActualizacion = DateTime.UtcNow;
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("admin", $"Actualizó cliente: {cliente.Nombre} (ID: {cliente.Id})", string.Empty);
            return cliente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return false;

            // CORREGIDO: Verificar si hay movimientos asociados
            var movimientosAsociados = await _context.Movimientos.CountAsync(m => m.ClienteId == id);
            if (movimientosAsociados > 0)
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar el cliente '{cliente.Nombre}' porque tiene {movimientosAsociados} movimiento(s) asociado(s). " +
                    $"Los clientes con historial de transacciones no pueden eliminarse por razones de trazabilidad.");
            }

            // Si no hay movimientos, se puede eliminar
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("admin", $"Eliminó cliente: {cliente.Nombre} (ID: {id})", string.Empty);
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
