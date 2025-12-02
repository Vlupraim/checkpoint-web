using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Representa un procedimiento o proceso operativo estándar (SOP)
    /// </summary>
    public class Procedimiento
    {
        [Key]
   public int Id { get; set; }

  [Required]
        [StringLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
[StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Descripcion { get; set; }

    /// <summary>
        /// Categoría: Calidad, Operaciones, Seguridad, Logística, Administrativo
 /// </summary>
        [StringLength(50)]
   public string? Categoria { get; set; }

        /// <summary>
        /// Versión del documento (1.0, 1.1, 2.0, etc.)
        /// </summary>
        [StringLength(20)]
        public string Version { get; set; } = "1.0";

        /// <summary>
   /// Estado: Borrador, Vigente, Revisión, Obsoleto
        /// </summary>
        [StringLength(20)]
    public string Estado { get; set; } = "Borrador";

   public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;  // CORREGIDO: Usar UTC

        public DateTime? FechaAprobacion { get; set; }

        public DateTime? FechaVigencia { get; set; }

        public DateTime? FechaRevision { get; set; }

        /// <summary>
   /// Usuario responsable del procedimiento
        /// </summary>
   [StringLength(450)]
      public string? ResponsableId { get; set; }

        /// <summary>
/// Usuario que aprobó el procedimiento
        /// </summary>
 [StringLength(450)]
        public string? AprobadoPor { get; set; }

        /// <summary>
        /// Ruta al documento PDF o archivo adjunto
        /// </summary>
        [StringLength(500)]
        public string? RutaDocumento { get; set; }

     [StringLength(2000)]
public string? Observaciones { get; set; }

        /// <summary>
      /// Frecuencia de revisión en meses
        /// </summary>
 public int? FrecuenciaRevisionMeses { get; set; }

        public bool Activo { get; set; } = true;
    }
}
