using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using System;
using System.Security.Claims;

namespace checkpoint_web.Pages.Bodega
{
 // CORREGIDO: Solo PersonalBodega puede recepcionar (Admin solo ve)
 [Authorize(Roles = "PersonalBodega")]
 public class RecepcionModel : PageModel
 {
 private readonly IProductoService _productoService;
 private readonly IUbicacionService _ubicacionService;
 private readonly CheckpointDbContext _context;
 private readonly IAuditService _auditService;
 public RecepcionModel(IProductoService productoService, IUbicacionService ubicacionService, CheckpointDbContext context, IAuditService auditService)
 {
 (_productoService, _ubicacionService, _context, _auditService) = (productoService, ubicacionService, context, auditService);
 }

 [BindProperty]
 public Guid ProductoId { get; set; }
 [BindProperty]
 public string CodigoLote { get; set; } = string.Empty;
 [BindProperty]
 public DateTime FechaIngreso { get; set; } = DateTime.Today;
 [BindProperty]
 public DateTime? FechaVencimiento { get; set; } = DateTime.Today.AddDays(180);
 [BindProperty]
 public Guid UbicacionId { get; set; }
 [BindProperty]
 public decimal Cantidad { get; set; }
 [BindProperty]
 public decimal TempIngreso { get; set; }

 public SelectList Productos { get; set; } = new SelectList(new List<Producto>(), "Id", "Nombre");
 public SelectList Ubicaciones { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");

 public async Task OnGetAsync()
 {
 var productos = await _productoService.GetAllAsync();
 Productos = new SelectList(productos, "Id", "Nombre");
 var ubicaciones = await _ubicacionService.GetAllAsync();
 Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 var productos = await _productoService.GetAllAsync();
 Productos = new SelectList(productos, "Id", "Nombre");
 var ubicaciones = await _ubicacionService.GetAllAsync();
 Ubicaciones = new SelectList(ubicaciones.Select(u => new { u.Id, Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" }), "Id", "Codigo");
 return Page();
 }

 // CRÍTICO: Create Lote con estado inicial CUARENTENA
 // El lote NO puede usarse hasta que Calidad lo libere
 var lote = new Lote
 {
 Id = Guid.NewGuid(),
 ProductoId = ProductoId,
 CodigoLote = CodigoLote,
 FechaIngreso = FechaIngreso,
 FechaVencimiento = FechaVencimiento,
 TempIngreso = TempIngreso,
 CantidadInicial = Cantidad,
 CantidadDisponible = Cantidad,
 Estado = EstadoLote.Cuarentena  // ? ESTADO INICIAL: Cuarentena (esperando revisión de Calidad)
 };
 _context.Lotes.Add(lote);

 // Create initial Stock
 var stock = new Stock
 {
 Id = Guid.NewGuid(),
 LoteId = lote.Id,
 UbicacionId = UbicacionId,
 Cantidad = Cantidad,
 ActualizadoEn = DateTime.UtcNow
 };
 _context.Stocks.Add(stock);

 await _context.SaveChangesAsync();

 // Use NameIdentifier claim for user id (consistent with other services)
 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
 // Fallback to system if empty (AuditService ignores anonymous/system if configured)
 if (string.IsNullOrEmpty(userId)) userId = "system";

 await _auditService.LogAsync(userId,
 $"Recepcion: Lote={CodigoLote}, Cantidad={Cantidad}, Estado={EstadoLote.Cuarentena}",
 System.Text.Json.JsonSerializer.Serialize(new { lote, stock }));

 TempData["Message"] = $"Recepción registrada correctamente. Lote en CUARENTENA esperando revisión de Calidad.";
 return RedirectToPage();
 }
 }
}
