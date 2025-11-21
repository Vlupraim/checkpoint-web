using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Calidad.ControlCalidad
{
    [Authorize(Roles = "ControlCalidad")]
   public class IndexModel : PageModel
    {
     private readonly ICalidadService _calidadService;

     public IndexModel(ICalidadService calidadService)
        {
   _calidadService = calidadService;
   }

   public IEnumerable<Lote> LotesPendientes { get; set; } = new List<Lote>();
     public IEnumerable<CalidadLiberacion> Historial { get; set; } = new List<CalidadLiberacion>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();

 [BindProperty]
        public Guid LoteId { get; set; }
 [BindProperty]
  public string Accion { get; set; } = string.Empty;
        [BindProperty]
 public string Observacion { get; set; } = string.Empty;

public async Task OnGetAsync()
     {
   LotesPendientes = await _calidadService.GetLotesPendientesRevisionAsync();
     Historial = await _calidadService.GetHistorialCalidadAsync(30);
      Estadisticas = await _calidadService.GetEstadisticasAsync();
        }

  public async Task<IActionResult> OnPostAsync()
 {
            try
  {
      // CORREGIDO: Usar UserId en lugar de email
    var usuario = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "calidad";
 
    switch (Accion)
 {
      case "Aprobar":
       await _calidadService.AprobarLoteAsync(LoteId, usuario, Observacion);
  TempData["SuccessMessage"] = "? Lote aprobado y liberado para uso exitosamente";
 break;
case "Rechazar":
  if (string.IsNullOrWhiteSpace(Observacion))
  {
    TempData["ErrorMessage"] = "?? Debe especificar el motivo del rechazo";
  return RedirectToPage();
      }
 await _calidadService.RechazarLoteAsync(LoteId, usuario, Observacion);
        TempData["SuccessMessage"] = "? Lote rechazado correctamente";
   break;
   case "Bloquear":
    if (string.IsNullOrWhiteSpace(Observacion))
  {
  TempData["ErrorMessage"] = "?? Debe especificar el motivo del bloqueo";
return RedirectToPage();
  }
   await _calidadService.BloquearLoteAsync(LoteId, usuario, Observacion);
TempData["SuccessMessage"] = "?? Lote bloqueado para investigación";
      break;
       default:
   TempData["ErrorMessage"] = "? Acción no válida";
     break;
}
 }
  catch (Exception ex)
     {
   TempData["ErrorMessage"] = $"? Error: {ex.Message}";
  }

    return RedirectToPage();
 }
    }
}
