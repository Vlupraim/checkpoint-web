using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Sedes
{
 [Authorize(Roles = "Administrador")]
 public class IndexModel : PageModel
 {
 private readonly ISedeService _sedeService;
 public IndexModel(ISedeService sedeService) => _sedeService = sedeService;

 public IList<Sede> Sedes { get; set; } = new List<Sede>();

 public async Task OnGetAsync()
 {
 Sedes = await _sedeService.GetAllAsync();
 }
 }
}
