using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Services;

namespace checkpoint_web.ViewComponents
{
 public class NotificationsViewComponent : ViewComponent
    {
 private readonly INotificacionService _notificacionService;

     public NotificationsViewComponent(INotificacionService notificacionService)
        {
  _notificacionService = notificacionService;
        }

public async Task<IViewComponentResult> InvokeAsync()
    {
  var userId = UserClaimsPrincipal?.Identity?.Name ?? string.Empty;
  
if (string.IsNullOrEmpty(userId))
      return View(new NotificationsViewModel { Notificaciones = new List<checkpoint_web.Models.Notificacion>() });

   var notificaciones = await _notificacionService.GetNotificacionesUsuarioAsync(userId, soloNoLeidas: true);
     var count = await _notificacionService.GetCountNoLeidasAsync(userId);

 var viewModel = new NotificationsViewModel
  {
     Notificaciones = notificaciones.ToList(),
  Count = count
 };

  return View(viewModel);
   }
    }

    public class NotificationsViewModel
    {
        public List<checkpoint_web.Models.Notificacion> Notificaciones { get; set; } = new();
 public int Count { get; set; }
    }
}
