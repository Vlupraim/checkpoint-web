using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Services;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Bodega.Ajustes
{
    // CORREGIDO: Solo PersonalBodega puede crear ajustes
    [Authorize(Roles = "PersonalBodega")]
    public class CreateModel : PageModel
    {
  private readonly IMovimientoService _movimientoService;
        private readonly CheckpointDbContext _context;

   public CreateModel(IMovimientoService movimientoService, CheckpointDbContext context)
 {
  _movimientoService = movimientoService;
       _context = context;
  }

        [BindProperty]
        public Guid LoteId { get; set; }
        [BindProperty]
     public Guid UbicacionId { get; set; }
        [BindProperty]
 public decimal Cantidad { get; set; }
        [BindProperty]
 public string Motivo { get; set; } = string.Empty;

public SelectList? LotesSelectList { get; set; }
     public SelectList? UbicacionesSelectList { get; set; }

        public async Task OnGetAsync()
    {
await LoadSelectListsAsync();
        }

    public async Task<IActionResult> OnPostAsync()
      {
   if (!ModelState.IsValid)
 {
    await LoadSelectListsAsync();
       return Page();
 }

       try
  {
       await _movimientoService.CrearAjusteAsync(
   LoteId,
      UbicacionId,
       Cantidad,
       User.Identity?.Name ?? "unknown",
  Motivo
  );
     TempData["SuccessMessage"] = "Ajuste creado. Esperando aprobación de administrador.";
return RedirectToPage("./Index");
     }
 catch (Exception ex)
    {
       TempData["ErrorMessage"] = ex.Message;
await LoadSelectListsAsync();
   return Page();
   }
 }

private async Task LoadSelectListsAsync()
        {
 // Solo mostrar lotes que NO estén bloqueados
 var lotes = await _context.Lotes
  .Include(l => l.Producto)
      .Where(l => l.Estado != EstadoLote.Bloqueado)
  .Take(100)
        .ToListAsync();
  LotesSelectList = new SelectList(lotes, "Id", "CodigoLote");

  var ubicaciones = await _context.Ubicaciones.Include(u => u.Sede).ToListAsync();
  UbicacionesSelectList = new SelectList(ubicaciones, "Id", "Codigo");
 }
    }
}
