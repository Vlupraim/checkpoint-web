using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace checkpoint_web.Pages.Admin.AuditLogs
{
 [Authorize(Roles = "Administrador")]
 public class IndexModel : PageModel
 {
 private readonly CheckpointDbContext _context;
 public IndexModel(CheckpointDbContext context) => _context = context;

 public IList<AuditLog> Logs { get; set; } = new List<AuditLog>();

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
 }
 }
}
