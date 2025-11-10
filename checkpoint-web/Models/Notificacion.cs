using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Sistema de notificaciones para usuarios
    /// </summary>
  public class Notificacion
    {
        [Key]
 public int Id { get; set; }

        /// <summary>
      /// Usuario destinatario de la notificación
        /// </summary>
 [Required]
 [StringLength(450)]
 public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo: Alerta, Recordatorio, Información, Advertencia, Error, Éxito
 /// </summary>
    [Required]
        [StringLength(50)]
     public string Tipo { get; set; } = "Información";

        /// <summary>
        /// Título breve de la notificación
 /// </summary>
  [Required]
[StringLength(200)]
      public string Titulo { get; set; } = string.Empty;

      /// <summary>
/// Mensaje completo
        /// </summary>
        [StringLength(1000)]
        public string? Mensaje { get; set; }

   /// <summary>
        /// URL a la que redirige al hacer clic
        /// </summary>
 [StringLength(500)]
      public string? Url { get; set; }

     /// <summary>
   /// Categoría: Tarea, Inventario, Calidad, Sistema, Usuario
  /// </summary>
        [StringLength(50)]
        public string? Categoria { get; set; }

        /// <summary>
        /// Referencia al objeto relacionado (TareaId, LoteId, etc.)
        /// </summary>
        [StringLength(50)]
      public string? ReferenciaId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaLeida { get; set; }

        /// <summary>
        /// Fecha de vencimiento de la notificación (para recordatorios)
    /// </summary>
        public DateTime? FechaVencimiento { get; set; }

   public bool Leida { get; set; } = false;

  /// <summary>
        /// Prioridad: Baja, Media, Alta
     /// </summary>
    [StringLength(20)]
 public string Prioridad { get; set; } = "Media";

 /// <summary>
        /// Para notificaciones recurrentes o programadas
        /// </summary>
        public bool EsRecurrente { get; set; } = false;

  public bool Activa { get; set; } = true;

        // Navegación
        [ForeignKey(nameof(UsuarioId))]
public virtual ApplicationUser? Usuario { get; set; }
    }
}
