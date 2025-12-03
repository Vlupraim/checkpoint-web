using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
    public interface IMovimientoService
    {
        Task<IEnumerable<Movimiento>> GetAllAsync();
        Task<IEnumerable<Movimiento>> GetByTipoAsync(string tipo);
        Task<Movimiento?> GetByIdAsync(Guid id);
        Task<Movimiento> CrearIngresoAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId);
        Task<Movimiento> CrearTrasladoAsync(Guid loteId, Guid origenId, Guid destinoId, decimal cantidad, string usuarioId, string? motivo = null);
        Task<Movimiento> CrearSalidaAsync(Guid loteId, Guid ubicacionId, decimal cantidad, int? clienteId, string usuarioId, string? motivo = null);
        Task<Movimiento> CrearDevolucionAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId, string motivo);
        Task<Movimiento> CrearAjusteAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId, string motivo);
        Task<bool> AprobarAjusteAsync(Guid movimientoId, string aprobadoPor);
        Task<Dictionary<string, object>> GetEstadisticasAsync();
        Task<decimal> GetStockDisponibleEnUbicacionAsync(Guid loteId, Guid ubicacionId);
    }

    public class MovimientoService : IMovimientoService
    {
        private readonly CheckpointDbContext _context;
        private readonly IAuditService _auditService;

        public MovimientoService(CheckpointDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Movimiento>> GetAllAsync() =>
            await _context.Movimientos
                .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                .Include(m => m.OrigenUbicacion)
                .Include(m => m.DestinoUbicacion)
                .Include(m => m.Cliente)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

        public async Task<IEnumerable<Movimiento>> GetByTipoAsync(string tipo) =>
            await _context.Movimientos
                .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                .Include(m => m.OrigenUbicacion)
                .Include(m => m.DestinoUbicacion)
                .Where(m => m.Tipo == tipo)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

        public async Task<Movimiento?> GetByIdAsync(Guid id) =>
            await _context.Movimientos
                .Include(m => m.Lote).ThenInclude(l => l!.Producto)
                .Include(m => m.OrigenUbicacion)
                .Include(m => m.DestinoUbicacion)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<Movimiento> CrearIngresoAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId)
        {
            var stockAnterior = await GetStockDisponibleEnUbicacionAsync(loteId, ubicacionId);

            var movimiento = new Movimiento
            {
                Id = Guid.NewGuid(),
                LoteId = loteId,
                DestinoUbicacionId = ubicacionId,
                Tipo = "Ingreso",
                Cantidad = cantidad,
                UsuarioId = usuarioId,
                Estado = "Completado",
                StockAnterior = stockAnterior,
                StockPosterior = stockAnterior + cantidad
            };

            _context.Movimientos.Add(movimiento);
            await ActualizarStockAsync(loteId, ubicacionId, cantidad, esIncremento: true);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(usuarioId, $"Ingreso: {cantidad} unidades a lote {loteId}", $"Ubicacion: {ubicacionId}");
            return movimiento;
        }

        public async Task<Movimiento> CrearTrasladoAsync(Guid loteId, Guid origenId, Guid destinoId, decimal cantidad, string usuarioId, string? motivo = null)
        {
            // 1. Validar que el lote existe y está liberado
            var lote = await _context.Lotes
                .Include(l => l.Producto)
                .FirstOrDefaultAsync(l => l.Id == loteId);
            
            if (lote == null)
                throw new KeyNotFoundException($"Lote {loteId} no encontrado");
            
            if (lote.Estado != EstadoLote.Liberado)
                throw new InvalidOperationException($"El lote {lote.CodigoLote} no está liberado. Estado actual: {lote.Estado}");

            // 2. Validar stock origen
            var stockOrigen = await GetStockDisponibleEnUbicacionAsync(loteId, origenId);
            if (stockOrigen < cantidad)
                throw new InvalidOperationException($"Stock insuficiente en ubicación de origen. Disponible: {stockOrigen:N2}, Solicitado: {cantidad:N2}");

            var stockDestinoAntes = await GetStockDisponibleEnUbicacionAsync(loteId, destinoId);

            var movimiento = new Movimiento
            {
                Id = Guid.NewGuid(),
                LoteId = loteId,
                OrigenUbicacionId = origenId,
                DestinoUbicacionId = destinoId,
                Tipo = "Traslado",
                Cantidad = cantidad,
                Unidad = lote.Producto?.Unidad ?? "u",  // CORREGIDO: Usar unidad del producto
                UsuarioId = usuarioId,
                Motivo = motivo,
                Estado = "Completado",
                StockAnterior = stockOrigen,
                StockPosterior = stockOrigen - cantidad
            };

            _context.Movimientos.Add(movimiento);
            
            // 3. Actualizar stocks (el método ActualizarStockAsync ya valida)
            await ActualizarStockAsync(loteId, origenId, cantidad, esIncremento: false);
            await ActualizarStockAsync(loteId, destinoId, cantidad, esIncremento: true);
            
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(usuarioId, $"Traslado: {cantidad} unidades de lote {lote.CodigoLote}", 
                $"Origen: {origenId}, Destino: {destinoId}");
            
            return movimiento;
        }

        public async Task<Movimiento> CrearSalidaAsync(Guid loteId, Guid ubicacionId, decimal cantidad, int? clienteId, string usuarioId, string? motivo = null)
        {
            var stock = await GetStockDisponibleEnUbicacionAsync(loteId, ubicacionId);
            if (stock < cantidad)
                throw new InvalidOperationException($"Stock insuficiente. Disponible: {stock:N2}, Requerido: {cantidad:N2}");

            var movimiento = new Movimiento
            {
                Id = Guid.NewGuid(),
                LoteId = loteId,
                OrigenUbicacionId = ubicacionId,
                ClienteId = clienteId,
                Tipo = "Salida",
                Cantidad = cantidad,
                UsuarioId = usuarioId,
                Motivo = motivo,
                Estado = "Completado",
                StockAnterior = stock,
                StockPosterior = stock - cantidad
            };

            _context.Movimientos.Add(movimiento);
            await ActualizarStockAsync(loteId, ubicacionId, cantidad, esIncremento: false);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(usuarioId, $"Salida: {cantidad} unidades de lote {loteId}", $"Cliente: {clienteId}");
            return movimiento;
        }

        public async Task<Movimiento> CrearDevolucionAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId, string motivo)
        {
            var stockAntes = await GetStockDisponibleEnUbicacionAsync(loteId, ubicacionId);

            var movimiento = new Movimiento
            {
                Id = Guid.NewGuid(),
                LoteId = loteId,
                DestinoUbicacionId = ubicacionId,
                Tipo = "Devolucion",
                Cantidad = cantidad,
                UsuarioId = usuarioId,
                Motivo = motivo,
                Estado = "Completado",
                StockAnterior = stockAntes,
                StockPosterior = stockAntes + cantidad
            };

            _context.Movimientos.Add(movimiento);
            await ActualizarStockAsync(loteId, ubicacionId, cantidad, esIncremento: true);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(usuarioId, $"Devolución: {cantidad} unidades a lote {loteId}", motivo);
            return movimiento;
        }

        public async Task<Movimiento> CrearAjusteAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId, string motivo)
        {
            var stockAntes = await GetStockDisponibleEnUbicacionAsync(loteId, ubicacionId);

            // IMPORTANTE: Cambiar estado del lote a Cuarentena cuando se solicita ajuste
            var lote = await _context.Lotes.FindAsync(loteId);
            if (lote == null)
                throw new KeyNotFoundException($"Lote {loteId} no encontrado");
            
            var estadoAnterior = lote.Estado;
            lote.Estado = EstadoLote.Cuarentena;
            
            var movimiento = new Movimiento
            {
                Id = Guid.NewGuid(),
                LoteId = loteId,
                DestinoUbicacionId = cantidad > 0 ? ubicacionId : null,
                OrigenUbicacionId = cantidad < 0 ? ubicacionId : null,
                Tipo = "Ajuste",
                Cantidad = Math.Abs(cantidad),
                UsuarioId = usuarioId,
                Motivo = $"[Lote pasó de {estadoAnterior} a Cuarentena] {motivo}",
                Estado = "Pendiente", // Requiere aprobación
                StockAnterior = stockAntes,
                StockPosterior = stockAntes + cantidad
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(usuarioId, 
                $"Ajuste solicitado: {cantidad} unidades de lote {lote.CodigoLote}", 
                $"Lote cambiado a Cuarentena. Requiere aprobación de Control de Calidad y Administrador");
            
            return movimiento;
        }

        public async Task<bool> AprobarAjusteAsync(Guid movimientoId, string aprobadoPor)
        {
            var movimiento = await _context.Movimientos
                .Include(m => m.Lote)
                .FirstOrDefaultAsync(m => m.Id == movimientoId);
                
            if (movimiento == null || movimiento.Tipo != "Ajuste" || movimiento.Estado != "Pendiente")
                return false;

            // VALIDACIÓN CRÍTICA: El lote debe estar LIBERADO antes de aprobar el ajuste
            // Esto asegura que Control de Calidad ya lo revisó
            if (movimiento.Lote!.Estado != EstadoLote.Liberado)
            {
                throw new InvalidOperationException(
                    $"No se puede aprobar el ajuste. El lote {movimiento.Lote.CodigoLote} está en estado '{movimiento.Lote.Estado}'. " +
                    $"Control de Calidad debe liberarlo primero antes de que se pueda aprobar el ajuste numérico."
                );
            }

            movimiento.Estado = "Aprobado";
            movimiento.AprobadoPor = aprobadoPor;
            movimiento.FechaAprobacion = DateTime.UtcNow;

            // Aplicar cambio en stock
            var ubicacionId = movimiento.DestinoUbicacionId ?? movimiento.OrigenUbicacionId!.Value;
            var esIncremento = movimiento.DestinoUbicacionId.HasValue;
            await ActualizarStockAsync(movimiento.LoteId, ubicacionId, movimiento.Cantidad, esIncremento);

            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync(aprobadoPor, 
                $"Aprobó ajuste ID: {movimientoId} de lote {movimiento.Lote.CodigoLote}", 
                $"Stock actualizado. Lote estaba en estado Liberado.");
            
            return true;
        }

        public async Task<Dictionary<string, object>> GetEstadisticasAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            return new Dictionary<string, object>
            {
                ["TotalMovimientos"] = await _context.Movimientos.CountAsync(),
                ["MovimientosHoy"] = await _context.Movimientos.CountAsync(m => m.Fecha >= hoy),
                ["TrasladosPendientes"] = await _context.Movimientos.CountAsync(m => m.Tipo == "Traslado" && m.Estado == "Pendiente"),
                ["AjustesPendientes"] = await _context.Movimientos.CountAsync(m => m.Tipo == "Ajuste" && m.Estado == "Pendiente")
            };
        }

        public async Task<decimal> GetStockDisponibleEnUbicacionAsync(Guid loteId, Guid ubicacionId)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.LoteId == loteId && s.UbicacionId == ubicacionId);
            return stock?.Cantidad ?? 0;
        }

        private async Task ActualizarStockAsync(Guid loteId, Guid ubicacionId, decimal cantidad, bool esIncremento)
        {
            // Obtener el lote para conocer la unidad del producto
            var lote = await _context.Lotes
                .Include(l => l.Producto)
                .FirstOrDefaultAsync(l => l.Id == loteId);
            
            var unidad = lote?.Producto?.Unidad ?? "u";

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.LoteId == loteId && s.UbicacionId == ubicacionId);

            if (stock == null)
            {
                if (!esIncremento)
                    throw new InvalidOperationException("No se puede decrementar stock inexistente");

                stock = new Stock
                {
                    Id = Guid.NewGuid(),
                    LoteId = loteId,
                    UbicacionId = ubicacionId,
                    Cantidad = cantidad,
                    Unidad = unidad  // CORREGIDO: Usar unidad real del producto
                };
                _context.Stocks.Add(stock);
            }
            else
            {
                if (esIncremento)
                    stock.Incrementar(cantidad);
                else
                    stock.Decrementar(cantidad);  // Ahora valida automáticamente stock insuficiente
            }

            stock.ActualizadoEn = DateTime.UtcNow;
        }
    }
}
