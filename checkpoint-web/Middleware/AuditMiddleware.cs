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

        public async Task InvokeAsync(HttpContext context)
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
  
// CRÍTICO: Capturar el IServiceProvider ANTES de que el context se cierre
      var serviceProvider = context.RequestServices;
  
            // Ejecutar el request
        await _next(context);

      // DESPUÉS intentar auditar (async, sin bloquear)
    // Solo audit non-static and non-GET requests
            if (method != HttpMethods.Get &&
          !path.StartsWith("/lib") &&
       !path.StartsWith("/css") &&
         !path.StartsWith("/js") &&
       !path.StartsWith("/debug") &&
           !path.StartsWith("/health") &&
       !path.StartsWith("/api/session") && // NO auditar /api/session/end
         !string.IsNullOrEmpty(userId))
   {
      var action = $"{method} {path}";
           
 // Fire and forget con el serviceProvider capturado
  _ = Task.Run(async () =>
          {
  try
     {
             // Crear un nuevo scope con el serviceProvider CAPTURADO (no del HttpContext)
               using var scope = serviceProvider.CreateScope();
    var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
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