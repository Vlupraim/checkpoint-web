using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Ubicaciones
{
 [Authorize(Roles = "Administrador")]
 public class IndexModel : PageModel
 {
 private readonly IUbicacionService _ubicacionService;
 public IndexModel(IUbicacionService ubicacionService) => _ubicacionService = ubicacionService;

 public IList<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();

 public async Task OnGetAsync()
 {
 Ubicaciones = await _ubicacionService.GetAllAsync();
 }
 }
}
