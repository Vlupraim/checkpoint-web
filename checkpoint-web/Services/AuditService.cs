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
 // CRÍTICO: NO intentar guardar logs para usuarios anonymous o sin autenticar
       // Esto evita violaciones de FK constraint con AspNetUsers
            if (string.IsNullOrEmpty(userId) || 
      userId == "anonymous" || 
    userId == "system")
     {
    // Ignorar silenciosamente - estos no son usuarios reales en AspNetUsers
    return;
         }
      
            var entry = new AuditLog 
       { 
    Id = Guid.NewGuid(), 
         UserId = userId, 
       Action = action, 
                Details = details,
       Timestamp = DateTime.UtcNow  // CRÍTICO: Usar UTC explícitamente
            };
        
        _context.AuditLogs.Add(entry);
   await _context.SaveChangesAsync();
 }
 }
}