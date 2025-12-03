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

 [BindProperty(SupportsGet = true)]
 public string? UserFilter { get; set; }

 [BindProperty(SupportsGet = true)]
 public string? ActionFilter { get; set; }

 [BindProperty(SupportsGet = true)]
 public DateTime? From { get; set; }

 [BindProperty(SupportsGet = true)]
 public DateTime? To { get; set; }

 public async Task OnGetAsync()
 {
 var q = _context.AuditLogs.AsNoTracking().AsQueryable();
         
            if (!string.IsNullOrWhiteSpace(UserFilter))
            {
 q = q.Where(a => a.UserId.Contains(UserFilter));
            }
            
   if (!string.IsNullOrWhiteSpace(ActionFilter))
    {
 q = q.Where(a => a.Action.Contains(ActionFilter));
            }
       
            if (From.HasValue)
            {
 q = q.Where(a => a.Timestamp >= From.Value);
            }
       
            if (To.HasValue)
        {
            // include the whole day
 var end = To.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(a => a.Timestamp <= end);
      }
   
 Logs = await q.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();
    
      // Resolver nombres de usuario
        var userIds = Logs.Select(l => l.UserId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
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
    }
}
