using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using checkpoint_web.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
  
            // Determinar si debemos auditar ANTES de ejecutar el request
            bool shouldAudit = method != HttpMethods.Get &&
                !path.StartsWith("/lib") &&
                !path.StartsWith("/css") &&
                !path.StartsWith("/js") &&
                !path.StartsWith("/debug") &&
                !path.StartsWith("/health") &&
                !path.StartsWith("/api/session") &&
                !path.StartsWith("/Error") && // NO auditar /Error
                !string.IsNullOrEmpty(userId);

            // Ejecutar el request
            await _next(context);

            // DESPUÉS del request, auditar SINCRÓNICAMENTE (ANTES de que el scope se cierre)
            if (shouldAudit)
            {
                var action = $"{method} {path}";
           
                try
                {
                    // ✅ SINCRÓNICO - Ejecutar ANTES de que el HttpContext se dispose
                    await auditService.LogAsync(userId!, action);
                }
                catch (Exception ex)
                {
                    // Solo log warning, no fallar el request
                    _logger.LogWarning(ex, "Error al escribir el registro de auditoría para el usuario {userId} {action}", userId, action);
                }
            }
        }
    }
}