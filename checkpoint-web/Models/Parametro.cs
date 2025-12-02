using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Configuración y parámetros generales del sistema
    /// </summary>
    public class Parametro
  {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Clave única del parámetro (ej: "StockMinimoGeneral", "DiasAlertaVencimiento")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

    [Required]
 [StringLength(200)]
     public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
   public string? Descripcion { get; set; }

        /// <summary>
        /// Valor del parámetro (puede ser número, texto, JSON)
        /// </summary>
  [Required]
        [StringLength(2000)]
   public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de dato: String, Integer, Decimal, Boolean, JSON
        /// </summary>
        [StringLength(20)]
        public string TipoDato { get; set; } = "String";

 /// <summary>
        /// Categoría: Sistema, Inventario, Calidad, Notificaciones, Seguridad
 /// </summary>
        [StringLength(50)]
        public string? Categoria { get; set; }

        /// <summary>
      /// Unidad de medida si aplica (días, %, cantidad, etc.)
        /// </summary>
        [StringLength(20)]
     public string? Unidad { get; set; }

        /// <summary>
        /// Indica si el parámetro puede ser modificado por el usuario
  /// </summary>
 public bool EsEditable { get; set; } = true;

 public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;  // CORREGIDO: Usar UTC

    public DateTime? UltimaModificacion { get; set; }

  [StringLength(450)]
        public string? ModificadoPor { get; set; }

  public bool Activo { get; set; } = true;
    }
}
