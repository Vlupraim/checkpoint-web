using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using checkpoint_web.Models;
using checkpoint_web.Services;
using checkpoint_web.Data;
using Microsoft.EntityFrameworkCore;

namespace checkpoint_web.Pages.Admin.Tareas
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
   private readonly ITareaService _tareaService;
    private readonly CheckpointDbContext _context;

public CreateModel(ITareaService tareaService, CheckpointDbContext context)
        {
   _tareaService = tareaService;
        _context = context;
        }

 [BindProperty]
 public Tarea Tarea { get; set; } = new();

        public SelectList? EstadosSelectList { get; set; }
        public SelectList? PrioridadesSelectList { get; set; }
public SelectList? TiposSelectList { get; set; }
 public SelectList? ProductosSelectList { get; set; }
 public SelectList? LotesSelectList { get; set; }

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

      Tarea.CreadoPor = User.Identity?.Name ?? "admin";
     await _tareaService.CreateAsync(Tarea);

     TempData["SuccessMessage"] = "Tarea creada exitosamente";
  return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
   {
       EstadosSelectList = new SelectList(new[] { "Pendiente", "EnProgreso", "Finalizada", "Cancelada", "Bloqueada" });
   PrioridadesSelectList = new SelectList(new[] { "Baja", "Media", "Alta", "Urgente" });
 TiposSelectList = new SelectList(new[] { "Operativa", "Administrativa", "Calidad", "Mantenimiento" });

         var productos = await _context.Productos.Where(p => p.Activo).ToListAsync();
 ProductosSelectList = new SelectList(productos, "Id", "Nombre");

var lotes = await _context.Lotes.Include(l => l.Producto).Take(100).ToListAsync();
      LotesSelectList = new SelectList(lotes, "Id", "CodigoLote");
 }
    }
}
