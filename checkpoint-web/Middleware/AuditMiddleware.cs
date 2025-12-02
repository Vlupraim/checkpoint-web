using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

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
            // Capturar información ANTES de ejecutar el request
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;
            string? userId = null;
            
            // Solo auditar si el usuario está autenticado
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
  
            // Ejecutar el request
            await _next(context);

            // DESPUÉS intentar auditar (fire and forget)
            // Solo audit non-static and non-GET requests
            if (method != HttpMethods.Get &&
                !path.StartsWith("/lib") &&
                !path.StartsWith("/css") &&
                !path.StartsWith("/js") &&
                !path.StartsWith("/debug") &&
                !path.StartsWith("/health") &&
                !path.StartsWith("/api/session") &&
                !string.IsNullOrEmpty(userId))
            {
                var action = $"{method} {path}";
           
                // Fire and forget - NO crear scope aquí, usar el auditService inyectado
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await auditService.LogAsync(userId, action);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al escribir el registro de auditoría para el usuario {userId} {action}", userId, action);
                    }
                });
            }
        }
    }
}