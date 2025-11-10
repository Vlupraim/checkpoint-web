using System;
using System.Threading.Tasks;
using checkpoint_web.Models;

namespace checkpoint_web.Services
{
 public interface IInventarioService
 {
 Task RegistrarIngresoAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId);
 Task RegistrarMovimientoInternoAsync(Guid loteId, Guid origenUbicacionId, Guid destinoUbicacionId, decimal cantidad, string usuarioId);
 Task RegistrarSalidaAsync(Guid loteId, Guid ubicacionId, decimal cantidad, string usuarioId);
 Task<Stock?> GetStockAsync(Guid loteId, Guid ubicacionId);
 }
}