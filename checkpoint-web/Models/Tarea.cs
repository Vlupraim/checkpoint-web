using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Representa una tarea o proceso asignado a un usuario con seguimiento de estado
    /// </summary>
    public class Tarea
    {
        [Key]
     public int Id { get; set; }

        [Required]
        [StringLength(200)]
   public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
 public string? Descripcion { get; set; }

        /// <summary>
        /// Estados: Pendiente, EnProgreso, Finalizada, Cancelada, Bloqueada
  /// </summary>
        [Required]
     [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Prioridad: Baja, Media, Alta, Urgente
        /// </summary>
        [StringLength(20)]
        public string Prioridad { get; set; } = "Media";

 /// <summary>
        /// Tipo: Operativa, Administrativa, Calidad, Mantenimiento
        /// </summary>
        [StringLength(50)]
        public string? Tipo { get; set; }

   [Required]
public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaLimite { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        /// <summary>
        /// Usuario que creó la tarea
        /// </summary>
        [StringLength(450)]
        public string? CreadoPor { get; set; }

        /// <summary>
        /// Usuario responsable de ejecutar la tarea
        /// </summary>
        [StringLength(450)]
  public string? ResponsableId { get; set; }

    /// <summary>
    /// Progreso de 0 a 100
        /// </summary>
        [Range(0, 100)]
        public int Progreso { get; set; } = 0;

        /// <summary>
    /// Notas o comentarios adicionales
        /// </summary>
        [StringLength(2000)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Relación opcional con Producto (para tareas de inventario)
        /// </summary>
   public Guid? ProductoId { get; set; }
  [ForeignKey(nameof(ProductoId))]
        public virtual Producto? Producto { get; set; }

     /// <summary>
/// Relación opcional con Lote (para tareas de calidad)
/// </summary>
        public Guid? LoteId { get; set; }
 [ForeignKey(nameof(LoteId))]
 public virtual Lote? Lote { get; set; }

  /// <summary>
     /// Historial de cambios en formato JSON o texto
        /// </summary>
  public string? Historial { get; set; }

        public bool Activo { get; set; } = true;
    }
}
