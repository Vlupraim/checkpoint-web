using System;
using System.Threading.Tasks;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Services
{
 public class InventarioService : IInventarioService
 {
 private readonly CheckpointDbContext _context;
 private readonly IAuditService _auditService;
 public InventarioService(CheckpointDbContext context, IAuditService auditService)
 {
 _context = context;
 _auditService = auditService;
 }

 public async Task RegistrarIngresoAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId)
 {
 // VALIDACIÓN CRÍTICA: Solo lotes LIBERADOS
 var lote = await _context.Lotes.FindAsync(loteId);
 if (lote == null)
  throw new KeyNotFoundException($"Lote {loteId} no encontrado");
 
 if (lote.Estado != EstadoLote.Liberado)
  throw new InvalidOperationException(
    $"? No se puede registrar ingreso del lote {lote.CodigoLote}. " +
    $"Estado actual: {lote.Estado}. " +
    $"El lote debe estar LIBERADO por Control de Calidad.");

 var stock = await GetStockAsync(loteId, ubicacionId);
 if (stock == null)
 {
 stock = new Stock
 {
 Id = Guid.NewGuid(),
 LoteId = loteId,
 UbicacionId = ubicacionId,
 Cantidad = cantidad,
 ActualizadoEn = DateTime.UtcNow
 };
 _context.Stocks.Add(stock);
 }
 else
 {
 stock.Cantidad += cantidad;
 stock.ActualizadoEn = DateTime.UtcNow;
 }

 lote.CantidadDisponible += cantidad;

 var movimiento = new Movimiento
 {
 Id = Guid.NewGuid(),
 LoteId = loteId,
 Tipo = "Ingreso",
 OrigenUbicacionId = ubicacionId,
 DestinoUbicacionId = ubicacionId,
 Cantidad = cantidad,
 Fecha = DateTime.UtcNow,
 UsuarioId = usuarioId
 };
 _context.Movimientos.Add(movimiento);
 await _context.SaveChangesAsync();
 await _auditService.LogAsync(usuarioId, $"Ingreso: Lote={loteId}, Cantidad={cantidad}");
 }

 public async Task RegistrarMovimientoInternoAsync(Guid loteId, Guid origenUbicacionId, Guid destinoUbicacionId, decimal cantidad, string usuarioId)
 {
 // VALIDACIÓN CRÍTICA: Solo lotes LIBERADOS
 var lote = await _context.Lotes.FindAsync(loteId);
 if (lote == null)
  throw new KeyNotFoundException($"Lote {loteId} no encontrado");
 
 if (lote.Estado != EstadoLote.Liberado)
  throw new InvalidOperationException(
    $"? No se puede mover el lote {lote.CodigoLote}. " +
    $"Estado actual: {lote.Estado}. " +
    $"Solo lotes LIBERADOS pueden moverse entre ubicaciones.");

 var stockOrigen = await GetStockAsync(loteId, origenUbicacionId);
 if (stockOrigen == null || stockOrigen.Cantidad < cantidad)
  throw new InvalidOperationException("Stock insuficiente en ubicación origen");

 stockOrigen.Cantidad -= cantidad;
 stockOrigen.ActualizadoEn = DateTime.UtcNow;

 var stockDestino = await GetStockAsync(loteId, destinoUbicacionId);
 if (stockDestino == null)
 {
 stockDestino = new Stock
 {
 Id = Guid.NewGuid(),
 LoteId = loteId,
 UbicacionId = destinoUbicacionId,
 Cantidad = cantidad,
 ActualizadoEn = DateTime.UtcNow
 };
 _context.Stocks.Add(stockDestino);
 }
 else
 {
 stockDestino.Cantidad += cantidad;
 stockDestino.ActualizadoEn = DateTime.UtcNow;
 }

 var movimiento = new Movimiento
 {
 Id = Guid.NewGuid(),
 LoteId = loteId,
 Tipo = "MovimientoInterno",
 OrigenUbicacionId = origenUbicacionId,
 DestinoUbicacionId = destinoUbicacionId,
 Cantidad = cantidad,
 Fecha = DateTime.UtcNow,
 UsuarioId = usuarioId
 };
 _context.Movimientos.Add(movimiento);
 await _context.SaveChangesAsync();
 await _auditService.LogAsync(usuarioId, $"MovimientoInterno: Lote={loteId}, Cantidad={cantidad}");
 }

 public async Task RegistrarSalidaAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId)
 {
 // VALIDACIÓN CRÍTICA: Solo lotes LIBERADOS
 var lote = await _context.Lotes.FindAsync(loteId);
 if (lote == null)
  throw new KeyNotFoundException($"Lote {loteId} no encontrado");
 
 if (lote.Estado != EstadoLote.Liberado)
  throw new InvalidOperationException(
 $"? No se puede registrar salida del lote {lote.CodigoLote}. " +
    $"Estado actual: {lote.Estado}. " +
 $"Solo lotes LIBERADOS pueden usarse para producción/venta.");

 var stock = await GetStockAsync(loteId, ubicacionId);
 if (stock == null || stock.Cantidad < cantidad)
  throw new InvalidOperationException($"Stock insuficiente. Disponible: {stock?.Cantidad ?? 0}, Solicitado: {cantidad}");

 stock.Cantidad -= cantidad;
 stock.ActualizadoEn = DateTime.UtcNow;

 lote.CantidadDisponible -= cantidad;

 var movimiento = new Movimiento
 {
 Id = Guid.NewGuid(),
 LoteId = loteId,
 Tipo = "Salida",
 OrigenUbicacionId = ubicacionId,
 DestinoUbicacionId = null,
 Cantidad = cantidad,
 Fecha = DateTime.UtcNow,
 UsuarioId = usuarioId
 };
 _context.Movimientos.Add(movimiento);
 await _context.SaveChangesAsync();
 await _auditService.LogAsync(usuarioId, $"Salida: Lote={loteId}, Cantidad={cantidad}");
 }

 public async Task<Stock?> GetStockAsync(Guid loteId, Guid ubicacionId)
 {
 return await _context.Stocks
 .FirstOrDefaultAsync(s => s.LoteId == loteId && s.UbicacionId == ubicacionId);
 }
 }
}