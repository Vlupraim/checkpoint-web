using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Clientes
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
  {
    private readonly IClienteService _clienteService;

public EditModel(IClienteService clienteService) => _clienteService = clienteService;

     [BindProperty]
        public Cliente Cliente { get; set; } = new();

   public async Task<IActionResult> OnGetAsync(int id)
 {
var cliente = await _clienteService.GetByIdAsync(id);
if (cliente == null) return NotFound();
      Cliente = cliente;
   return Page();
  }

        public async Task<IActionResult> OnPostAsync()
        {
   if (!ModelState.IsValid) return Page();
    await _clienteService.UpdateAsync(Cliente);
  TempData["SuccessMessage"] = "Cliente actualizado exitosamente";
  return RedirectToPage("./Index");
}
    }
}
