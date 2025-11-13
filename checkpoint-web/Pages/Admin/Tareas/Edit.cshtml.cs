using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
 private readonly ITareaService _tareaService;
        private readonly CheckpointDbContext _context;

  public EditModel(ITareaService tareaService, CheckpointDbContext context)
  {
 _tareaService = tareaService;
    _context = context;
  }

        [BindProperty]
public Tarea Tarea { get; set; } = new();
  public SelectList? EstadosSelectList { get; set; }
        public SelectList? PrioridadesSelectList { get; set; }
        public SelectList? UsuariosSelectList { get; set; }
   public List<TareaComentario> Comentarios { get; set; } = new();

     public async Task<IActionResult> OnGetAsync(int id)
      {
  var tarea = await _tareaService.GetByIdAsync(id);
    if (tarea == null)
            return NotFound();

   Tarea = tarea;
    await LoadSelectListsAsync();
         await LoadComentariosAsync(id);
   return Page();
        }

     public async Task<IActionResult> OnPostAsync()
     {
       if (!ModelState.IsValid)
  {
      await LoadSelectListsAsync();
   await LoadComentariosAsync(Tarea.Id);
     return Page();
  }

   // CORREGIR: Convertir FechaLimite a UTC si tiene valor
      if (Tarea.FechaLimite.HasValue && Tarea.FechaLimite.Value.Kind == DateTimeKind.Unspecified)
  {
         Tarea.FechaLimite = DateTime.SpecifyKind(Tarea.FechaLimite.Value, DateTimeKind.Utc);
    }

    await _tareaService.UpdateAsync(Tarea);
     TempData["SuccessMessage"] = "Tarea actualizada exitosamente";
  return RedirectToPage("./Index");
      }

   public async Task<IActionResult> OnPostAgregarComentarioAsync(int tareaId, string contenido)
    {
    if (string.IsNullOrWhiteSpace(contenido))
   {
  TempData["ErrorMessage"] = "El comentario no puede estar vacío";
      return RedirectToPage(new { id = tareaId });
  }

var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

   var comentario = new TareaComentario
  {
   TareaId = tareaId,
     UsuarioId = usuarioId,
   Contenido = contenido.Trim(),
 FechaCreacion = DateTime.UtcNow,
        Activo = true
   };

 _context.TareaComentarios.Add(comentario);
  await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Comentario agregado exitosamente";
     return RedirectToPage(new { id = tareaId });
        }

  private async Task LoadSelectListsAsync()
     {
  EstadosSelectList = new SelectList(new[] { "Pendiente", "EnProgreso", "Finalizada", "Cancelada", "Bloqueada" });
    PrioridadesSelectList = new SelectList(new[] { "Baja", "Media", "Alta", "Urgente" });
    
 // Cargar usuarios activos
       var usuarios = await _context.Users
   .Where(u => u.Activo)
    .OrderBy(u => u.Nombre)
       .Select(u => new { u.Id, Display = u.Nombre + " (" + u.Email + ")" })
   .ToListAsync();
      UsuariosSelectList = new SelectList(usuarios, "Id", "Display");
     }

      private async Task LoadComentariosAsync(int tareaId)
   {
  Comentarios = await _context.TareaComentarios
   .Include(c => c.Usuario)
  .Where(c => c.TareaId == tareaId && c.Activo)
     .OrderByDescending(c => c.FechaCreacion)
   .ToListAsync();
        }
    }
}
