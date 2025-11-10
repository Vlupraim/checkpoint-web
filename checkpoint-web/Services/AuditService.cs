using System.Threading.Tasks;
using checkpoint_web.Data;
using checkpoint_web.Models;

namespace checkpoint_web.Services
{
 public class AuditService : IAuditService
 {
 private readonly CheckpointDbContext _context;
 public AuditService(CheckpointDbContext context) => _context = context;

 public async Task LogAsync(string userId, string action, string? details = null)
 {
 var entry = new AuditLog { Id = Guid.NewGuid(), UserId = userId ?? string.Empty, Action = action, Details = details };
 _context.AuditLogs.Add(entry);
 await _context.SaveChangesAsync();
 }
 }
}