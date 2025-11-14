using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Models;
using checkpoint_web.Services;

namespace checkpoint_web.Pages.Admin.Clientes
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly IClienteService _clienteService;

        public IndexModel(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public IEnumerable<Cliente> Clientes { get; set; } = new List<Cliente>();
        public Dictionary<string, int> Estadisticas { get; set; } = new();

        [BindProperty]
        public Cliente NuevoCliente { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = await _clienteService.GetAllAsync();
            Estadisticas = await _clienteService.GetEstadisticasAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Clientes = await _clienteService.GetAllAsync();
                Estadisticas = await _clienteService.GetEstadisticasAsync();
                return Page();
            }

            await _clienteService.CreateAsync(NuevoCliente);
            TempData["SuccessMessage"] = "Cliente creado exitosamente";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var result = await _clienteService.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Cliente no encontrado o ya eliminado.";
                    return RedirectToPage();
                }

                TempData["SuccessMessage"] = "Cliente eliminado exitosamente";
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el cliente: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}
