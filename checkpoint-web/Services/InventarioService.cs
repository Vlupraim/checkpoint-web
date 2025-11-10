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
 public InventarioService(CheckpointDbContext context) => _context = context;

 public async Task<Stock?> GetStockAsync(Guid loteId, Guid ubicacionId)
 {
 return await _context.Stocks.FirstOrDefaultAsync(s => s.LoteId == loteId && s.UbicacionId == ubicacionId);
 }

 public async Task RegistrarIngresoAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId)
 {
 var stock = await GetStockAsync(loteId, ubicacionId);
 if (stock == null)
 {
 stock = new Stock { Id = Guid.NewGuid(), LoteId = loteId, UbicacionId = ubicacionId, Cantidad = cantidad };
 _context.Stocks.Add(stock);
 }
 else
 {
 stock.Incrementar(cantidad);
 }
 // also update lote available
 var lote = await _context.Lotes.FindAsync(loteId);
 if (lote != null)
 {
 lote.CantidadDisponible += cantidad;
 }
 await _context.SaveChangesAsync();
 }

 public async Task RegistrarMovimientoInternoAsync(Guid loteId, Guid origenUbicacionId, Guid destinoUbicacionId, decimal cantidad, string usuarioId)
 {
 if (origenUbicacionId == destinoUbicacionId) throw new InvalidOperationException("Origen y destino iguales");
 var origen = await GetStockAsync(loteId, origenUbicacionId);
 if (origen == null || origen.Cantidad < cantidad) throw new InvalidOperationException("Stock insuficiente en origen");
 origen.Decrementar(cantidad);
 var destino = await GetStockAsync(loteId, destinoUbicacionId);
 if (destino == null)
 {
 destino = new Stock { Id = Guid.NewGuid(), LoteId = loteId, UbicacionId = destinoUbicacionId, Cantidad = cantidad };
 _context.Stocks.Add(destino);
 }
 else
 {
 destino.Incrementar(cantidad);
 }
 // register movimiento
 var movimiento = new Movimiento { Id = Guid.NewGuid(), LoteId = loteId, OrigenUbicacionId = origenUbicacionId, DestinoUbicacionId = destinoUbicacionId, Cantidad = cantidad, Tipo = "Interno", Fecha = DateTime.UtcNow, UsuarioId = usuarioId };
 _context.Movimientos.Add(movimiento);
 await _context.SaveChangesAsync();
 }

 public async Task RegistrarSalidaAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId)
 {
 var stock = await GetStockAsync(loteId, ubicacionId);
 if (stock == null || stock.Cantidad < cantidad) throw new InvalidOperationException("Stock insuficiente para salida");
 stock.Decrementar(cantidad);
 var lote = await _context.Lotes.FindAsync(loteId);
 if (lote != null)
 {
 lote.CantidadDisponible -= cantidad;
 if (lote.CantidadDisponible <0) lote.CantidadDisponible =0;
 }
 var movimiento = new Movimiento { Id = Guid.NewGuid(), LoteId = loteId, OrigenUbicacionId = ubicacionId, DestinoUbicacionId = null, Cantidad = cantidad, Tipo = "Salida", Fecha = DateTime.UtcNow, UsuarioId = usuarioId };
 _context.Movimientos.Add(movimiento);
 await _context.SaveChangesAsync();
 }
 }
}