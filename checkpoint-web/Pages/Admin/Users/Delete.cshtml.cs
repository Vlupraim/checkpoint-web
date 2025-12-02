using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Users
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public DeleteModel(UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET request redirects to Index page
        public IActionResult OnGet(string? id)
        {
            return RedirectToPage("Index");
        }

        // POST request to delete user
        public async Task<IActionResult> OnPostAsync(string? id)
        {
            // Accept id from either route parameter or query string
            if (string.IsNullOrEmpty(id))
            {
                id = Request.Query["id"].ToString();
            }

            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID de usuario no válido";
                return RedirectToPage("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado";
                return RedirectToPage("Index");
            }

            // No permitir eliminarse a sí mismo
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propio usuario";
                return RedirectToPage("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                // Auditar la eliminación
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await _auditService.LogAsync(
                    userId,
                    $"DeleteUser:{id}",
                    $"Usuario eliminado: {user.Email}"
                );

                TempData["SuccessMessage"] = $"Usuario {user.Email} eliminado exitosamente";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al eliminar usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToPage("Index");
        }
    }
}
