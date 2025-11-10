using System.Threading.Tasks;

namespace checkpoint_web.Services
{
 public interface IAuditService
 {
 Task LogAsync(string userId, string action, string? details = null);
 }
}