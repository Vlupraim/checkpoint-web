using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Estados posibles de un lote en el flujo de control de calidad.
    /// Según diagrama de flujo del sistema.
    /// </summary>
    public enum EstadoLote
    {
        /// <summary>
        /// Estado inicial al recepcionar mercadería.
        /// El lote está en cuarentena esperando revisión de Calidad.
        /// </summary>
        [Display(Name = "Cuarentena")]
        Cuarentena = 0,

        /// <summary>
        /// El equipo de Calidad está revisando el lote.
        /// </summary>
        [Display(Name = "En Revisión")]
        EnRevision = 1,

        /// <summary>
        /// El lote fue aprobado por Calidad y puede usarse en operaciones.
        /// ÚNICO estado que permite movimientos/salidas de inventario.
     /// </summary>
        [Display(Name = "Liberado")]
        Liberado = 2,

        /// <summary>
        /// El lote fue rechazado por Calidad (no cumple especificaciones).
        /// No puede usarse.
        /// </summary>
        [Display(Name = "Rechazado")]
        Rechazado = 3,

        /// <summary>
        /// El lote está bloqueado para investigación.
      /// Requiere análisis adicional antes de tomar decisión final.
        /// </summary>
        [Display(Name = "Bloqueado")]
        Bloqueado = 4
    }
}
