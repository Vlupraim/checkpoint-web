using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;

namespace checkpoint_web.Middleware
{
 public class AuditMiddleware
 {
 private readonly RequestDelegate _next;
 private readonly ILogger<AuditMiddleware> _logger;
 public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
 {
 _next = next;
 _logger = logger;
 }

 public async Task InvokeAsync(HttpContext context, IAuditService auditService)
 {
 // Only audit non-static and non-GET requests by default
 var path = context.Request.Path.Value ?? string.Empty;
 if (context.Request.Method != HttpMethods.Get && !path.StartsWith("/lib") && !path.StartsWith("/css") && !path.StartsWith("/js"))
 {
 var userId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name ?? string.Empty : "anonymous";
 var action = $"{context.Request.Method} {path}";
 try
 {
 await auditService.LogAsync(userId ?? string.Empty, action);
 }
 catch (Exception ex)
 {
 _logger.LogWarning(ex, "Failed to write audit log");
 }
 }
 await _next(context);
 }
 }
}