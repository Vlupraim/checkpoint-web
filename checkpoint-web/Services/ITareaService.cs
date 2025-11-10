using checkpoint_web.Models;

namespace checkpoint_web.Services
{
    public interface ITareaService
    {
        Task<IEnumerable<Tarea>> GetAllAsync();
   Task<IEnumerable<Tarea>> GetByUsuarioAsync(string usuarioId);
        Task<IEnumerable<Tarea>> GetPendientesAsync();
        Task<IEnumerable<Tarea>> GetPorEstadoAsync(string estado);
        Task<IEnumerable<Tarea>> GetProximasAVencerAsync(int dias = 7);
        Task<Tarea?> GetByIdAsync(int id);
        Task<Tarea> CreateAsync(Tarea tarea);
    Task<Tarea> UpdateAsync(Tarea tarea);
        Task<bool> DeleteAsync(int id);
Task<bool> CambiarEstadoAsync(int id, string nuevoEstado, string? usuarioId = null);
        Task<bool> AsignarResponsableAsync(int id, string responsableId);
 Task<bool> ActualizarProgresoAsync(int id, int progreso);
        Task<Dictionary<string, int>> GetEstadisticasAsync();
    }
}
