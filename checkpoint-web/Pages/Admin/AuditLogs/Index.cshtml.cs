using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace checkpoint_web.Pages.Admin.AuditLogs
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly CheckpointDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public IndexModel(CheckpointDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<AuditLog> Logs { get; set; } = new List<AuditLog>();
        
        // Diccionario para mapear UserIds a nombres de usuario
        public Dictionary<string, string> UserNames { get; set; } = new Dictionary<string, string>();

        // Propiedades de filtro
        [BindProperty(SupportsGet = true)]
        public string? UserFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ActionFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        // Propiedades de paginación
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public async Task OnGetAsync()
        {
            try
            {
                // Validar que PageNumber sea válido
                if (PageNumber < 1) PageNumber = 1;

                var query = _context.AuditLogs.AsNoTracking().AsQueryable();
                
                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(UserFilter))
                {
                    query = query.Where(a => a.UserId != null && a.UserId.Contains(UserFilter));
                }
                
                if (!string.IsNullOrWhiteSpace(ActionFilter))
                {
                    query = query.Where(a => a.Action.Contains(ActionFilter));
                }
            
                if (From.HasValue)
                {
                    // Asegurar que se use la fecha completa desde las 00:00:00
                    var fromDate = From.Value.Date;
                    query = query.Where(a => a.Timestamp >= fromDate);
                }
            
                if (To.HasValue)
                {
                    // Incluir todo el día hasta las 23:59:59
                    var toDate = To.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(a => a.Timestamp <= toDate);
                }

                // Obtener el total de registros ANTES de paginar
                TotalRecords = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

                // Si el número de página es mayor al total de páginas, ajustar
                if (PageNumber > TotalPages && TotalPages > 0)
                {
                    PageNumber = TotalPages;
                }

                // Aplicar paginación
                Logs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
        
                // Resolver nombres de usuario
                var userIds = Logs
                    .Where(l => !string.IsNullOrEmpty(l.UserId))
                    .Select(l => l.UserId!)
                    .Distinct()
                    .ToList();
                    
                foreach (var userId in userIds)
                {
                    if (userId == "system" || userId == "anonymous")
                    {
                        UserNames[userId] = userId;
                        continue;
                    }
                    
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        UserNames[userId] = $"{user.Nombre} ({user.Email})";
                    }
                    else
                    {
                        // Si el usuario fue eliminado - prevenir error de Substring
                        var shortId = userId.Length >= 8 ? userId.Substring(0, 8) : userId;
                        UserNames[userId] = $"Usuario eliminado ({shortId}...)";
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error, registrar y mostrar mensaje amigable
                TempData["ErrorMessage"] = $"Error al cargar registros de auditoría: {ex.Message}";
                Logs = new List<AuditLog>();
                TotalRecords = 0;
                TotalPages = 0;
            }
        }
    }
}
