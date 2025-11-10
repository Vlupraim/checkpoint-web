using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Calidad.ControlCalidad
{
    [Authorize(Roles = "Administrador,ControlCalidad")]
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
      var usuario = User.Identity?.Name ?? "calidad";
       
    switch (Accion)
 {
      case "Aprobar":
       await _calidadService.AprobarLoteAsync(LoteId, usuario, Observacion);
     TempData["SuccessMessage"] = "Lote aprobado exitosamente";
 break;
case "Rechazar":
        if (string.IsNullOrWhiteSpace(Observacion))
     {
    TempData["ErrorMessage"] = "Debe especificar el motivo del rechazo";
  return RedirectToPage();
      }
 await _calidadService.RechazarLoteAsync(LoteId, usuario, Observacion);
        TempData["SuccessMessage"] = "Lote rechazado";
   break;
   case "Bloquear":
    if (string.IsNullOrWhiteSpace(Observacion))
  {
    TempData["ErrorMessage"] = "Debe especificar el motivo del bloqueo";
return RedirectToPage();
  }
   await _calidadService.BloquearLoteAsync(LoteId, usuario, Observacion);
TempData["SuccessMessage"] = "Lote bloqueado";
      break;
       default:
   TempData["ErrorMessage"] = "Acción no válida";
     break;
}
 }
  catch (Exception ex)
     {
   TempData["ErrorMessage"] = ex.Message;
  }

    return RedirectToPage();
 }
    }
}
