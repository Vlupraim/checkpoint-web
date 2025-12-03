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
            await _context.Proveedores
                .Include(p => p.Lotes)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

        public async Task<IEnumerable<Proveedor>> GetActivosAsync() =>
            await _context.Proveedores.Where(p => p.Activo && p.Estado == "Activo").OrderBy(p => p.Nombre).ToListAsync();

        public async Task<Proveedor?> GetByIdAsync(int id) =>
            await _context.Proveedores.FindAsync(id);

        public async Task<Proveedor> CreateAsync(Proveedor proveedor)
        {
            proveedor.FechaRegistro = DateTime.UtcNow;
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            // CORREGIDO: Usar "system" en lugar de "admin" (se ignora en AuditService)
            await _auditService.LogAsync("system", $"Creó proveedor: {proveedor.Nombre} (ID: {proveedor.Id})", string.Empty);
            return proveedor;
        }

        public async Task<Proveedor> UpdateAsync(Proveedor proveedor)
        {
            var existing = await _context.Proveedores.FindAsync(proveedor.Id);
            if (existing == null)
                throw new InvalidOperationException("Proveedor no encontrado");

            // Actualizar propiedades manualmente
            existing.Nombre = proveedor.Nombre;
            existing.NombreComercial = proveedor.NombreComercial;
            existing.IdentificadorFiscal = proveedor.IdentificadorFiscal;
            existing.Direccion = proveedor.Direccion;
            existing.Ciudad = proveedor.Ciudad;
            existing.Pais = proveedor.Pais;
            existing.Telefono = proveedor.Telefono;
            existing.Email = proveedor.Email;
            existing.PersonaContacto = proveedor.PersonaContacto;
            existing.Categoria = proveedor.Categoria;
            existing.Calificacion = proveedor.Calificacion;
            existing.Estado = proveedor.Estado;
            existing.Observaciones = proveedor.Observaciones;
            existing.Activo = proveedor.Activo;
            existing.UltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("system", $"Actualizó proveedor: {proveedor.Nombre} (ID: {proveedor.Id})", string.Empty);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return false;

            // Desvincular lotes asociados (SetNull behavior)
            var lotesAsociados = await _context.Lotes.Where(l => l.ProveedorId == id).ToListAsync();
            if (lotesAsociados.Any())
            {
                foreach (var lote in lotesAsociados)
                {
                    lote.ProveedorId = null;
                }
                await _auditService.LogAsync("system",
                    $"Se desvincularon {lotesAsociados.Count} lote(s) del proveedor '{proveedor.Nombre}' antes de eliminarlo",
                    string.Empty);
            }

            // Eliminar el proveedor
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("system", $"Eliminó proveedor: {proveedor.Nombre} (ID: {id})", string.Empty);
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
