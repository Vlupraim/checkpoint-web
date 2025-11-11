using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using checkpoint_web.Models;
using checkpoint_web.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Users
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
   private readonly IAuditService _auditService;

        public CreateModel(
      UserManager<ApplicationUser> userManager,
      RoleManager<IdentityRole> roleManager,
        IAuditService auditService)
        {
          _userManager = userManager;
            _roleManager = roleManager;
 _auditService = auditService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [BindProperty]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        public List<string> AvailableRoles { get; set; } = new List<string>();

 public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
         AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
{
        AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

            if (!ModelState.IsValid)
 {
          return Page();
      }

          // Verificar si el email ya existe
         var existingUser = await _userManager.FindByEmailAsync(Input.Email);
 if (existingUser != null)
        {
         ErrorMessage = "Ya existe un usuario con ese email";
           return Page();
 }

   // Crear el usuario
       var user = new ApplicationUser
     {
            UserName = Input.Email,
                Email = Input.Email,
           Nombre = Input.Nombre,
 Activo = Input.Activo
 };

    var result = await _userManager.CreateAsync(user, Input.Password);

     if (!result.Succeeded)
       {
    ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
   return Page();
      }

   // Asignar roles seleccionados
 if (SelectedRoles != null && SelectedRoles.Any())
     {
         await _userManager.AddToRolesAsync(user, SelectedRoles);
            }

            // Auditar la creación
  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
  var rolesString = SelectedRoles != null && SelectedRoles.Any() ? string.Join(", ", SelectedRoles) : "Ninguno";
       await _auditService.LogAsync(
       userId,
     $"CreateUser:{user.Id}",
      $"Usuario creado: {user.Email}, Roles: {rolesString}"
  );

 TempData["SuccessMessage"] = $"Usuario {user.Email} creado exitosamente";
       return RedirectToPage("/Admin/Users/Index");
    }

        public class InputModel
        {
            [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre es requerido")]
     [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
      public string Nombre { get; set; } = string.Empty;

[Required(ErrorMessage = "La contraseña es requerida")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            [DataType(DataType.Password)]
     public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe confirmar la contraseña")]
 [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
   [DataType(DataType.Password)]
       public string ConfirmPassword { get; set; } = string.Empty;

         public bool Activo { get; set; } = true;
        }
    }
}
