using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Bodega.Movimientos
{
    [Authorize(Roles = "PersonalBodega")]
    public class IndexModel : PageModel
    {
private readonly IMovimientoService _movimientoService;
  private readonly CheckpointDbContext _context;

        public IndexModel(IMovimientoService movimientoService, CheckpointDbContext context)
        {
  _movimientoService = movimientoService;
  _context = context;
  }

    public IEnumerable<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public Dictionary<string, object> Estadisticas { get; set; } = new();
     public string? FiltroTipo { get; set; }

        public SelectList? LotesSelectList { get; set; }
     public SelectList? UbicacionesSelectList { get; set; }
     public SelectList? ClientesSelectList { get; set; }

        [BindProperty]
        public TrasladoInput Traslado { get; set; } = new();

        [BindProperty]
   public SalidaInput Salida { get; set; } = new();

        [BindProperty]
  public DevolucionInput Devolucion { get; set; } = new();

 public async Task OnGetAsync(string? tipo = null)
  {
    FiltroTipo = tipo;
   Movimientos = string.IsNullOrEmpty(tipo) 
       ? await _movimientoService.GetAllAsync()
   : await _movimientoService.GetByTipoAsync(tipo);

            Estadisticas = await _movimientoService.GetEstadisticasAsync();
      await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostTrasladoAsync()
 {
   try
 {
     await _movimientoService.CrearTrasladoAsync(
   Traslado.LoteId,
      Traslado.OrigenUbicacionId,
    Traslado.DestinoUbicacionId,
      Traslado.Cantidad,
      User.Identity?.Name ?? "unknown",
      Traslado.Motivo
   );
          TempData["SuccessMessage"] = "Traslado registrado exitosamente";
      }
       catch (Exception ex)
{
  TempData["ErrorMessage"] = ex.Message;
  }
            return RedirectToPage();
     }

    public async Task<IActionResult> OnPostSalidaAsync()
 {
       try
     {
 await _movimientoService.CrearSalidaAsync(
   Salida.LoteId,
  Salida.UbicacionId,
Salida.Cantidad,
  Salida.ClienteId,
   User.Identity?.Name ?? "unknown",
   Salida.Motivo
    );
TempData["SuccessMessage"] = "Salida registrada exitosamente";
      }
   catch (Exception ex)
   {
     TempData["ErrorMessage"] = ex.Message;
  }
         return RedirectToPage();
}

   public async Task<IActionResult> OnPostDevolucionAsync()
{
   try
 {
    await _movimientoService.CrearDevolucionAsync(
     Devolucion.LoteId,
  Devolucion.UbicacionId,
      Devolucion.Cantidad,
User.Identity?.Name ?? "unknown",
     Devolucion.Motivo ?? "Devolución"
       );
 TempData["SuccessMessage"] = "Devolución registrada exitosamente";
 }
  catch (Exception ex)
            {
    TempData["ErrorMessage"] = ex.Message;
  }
return RedirectToPage();
        }

    private async Task LoadSelectListsAsync()
        {
var lotes = await _context.Lotes
   .Include(l => l.Producto)
    .Where(l => l.Estado != EstadoLote.Bloqueado)
.Take(100)
.ToListAsync();
LotesSelectList = new SelectList(lotes, "Id", "CodigoLote");

   var ubicaciones = await _context.Ubicaciones
     .Include(u => u.Sede)
     .ToListAsync();
  UbicacionesSelectList = new SelectList(ubicaciones, "Id", "Codigo");

     var clientes = await _context.Clientes
.Where(c => c.Activo && c.Estado == "Activo")
     .ToListAsync();
ClientesSelectList = new SelectList(clientes, "Id", "Nombre");
 }

        public class TrasladoInput
        {
       public Guid LoteId { get; set; }
     public Guid OrigenUbicacionId { get; set; }
   public Guid DestinoUbicacionId { get; set; }
 public decimal Cantidad { get; set; }
     public string? Motivo { get; set; }
        }

        public class SalidaInput
   {
   public Guid LoteId { get; set; }
  public Guid UbicacionId { get; set; }
  public decimal Cantidad { get; set; }
   public int? ClienteId { get; set; }
          public string? Motivo { get; set; }
   }

        public class DevolucionInput
 {
 public Guid LoteId { get; set; }
    public Guid UbicacionId { get; set; }
       public decimal Cantidad { get; set; }
          public string? Motivo { get; set; }
   }
    }
}
