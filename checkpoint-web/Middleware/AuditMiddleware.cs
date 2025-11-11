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
            // Ejecutar el request PRIMERO
            await _next(context);

            // DESPUES intentar auditar (async, sin bloquear)
            // Solo audit non-static and non-GET requests
            var path = context.Request.Path.Value ?? string.Empty;
            if (context.Request.Method != HttpMethods.Get &&
                !path.StartsWith("/lib") &&
                !path.StartsWith("/css") &&
                !path.StartsWith("/js") &&
                !path.StartsWith("/debug") &&
                !path.StartsWith("/health"))
            {
                // Solo auditar si el usuario está autenticado (evita problemas con foreign key)
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    // CORREGIDO: Usar UserId (ClaimTypes.NameIdentifier) en lugar de User.Identity.Name (email)
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                    var action = $"{context.Request.Method} {path}";

                    // Fire and forget - no esperar ni bloquear
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await auditService.LogAsync(userId, action);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al escribir el registro de auditoria para el usuario {userId} {action}", userId, action);
                        }
                    });
                }
            }
        }
    }
}