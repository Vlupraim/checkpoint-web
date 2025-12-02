using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Representa un proveedor de productos o materiales
    /// </summary>
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

 [Required]
        [StringLength(200)]
public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NombreComercial { get; set; }

        /// <summary>
        /// RUT, NIT, RFC, o identificador fiscal según país
    /// </summary>
        [StringLength(50)]
        public string? IdentificadorFiscal { get; set; }

 [StringLength(500)]
        public string? Direccion { get; set; }

 [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(50)]
     public string? Pais { get; set; } = "Chile";

        [StringLength(20)]
        public string? Telefono { get; set; }

     [StringLength(100)]
        [EmailAddress]
  public string? Email { get; set; }

[StringLength(100)]
        public string? PersonaContacto { get; set; }

        /// <summary>
      /// Categoría: MaterialPrima, Embalaje, Servicios, Logística
        /// </summary>
     [StringLength(50)]
        public string? Categoria { get; set; }

        /// <summary>
        /// Calificación de 1 a 5 estrellas
        /// </summary>
        [Range(0, 5)]
public int? Calificacion { get; set; }

        /// <summary>
        /// Estado: Activo, Inactivo, Suspendido, Homologado
        /// </summary>
        [StringLength(20)]
public string Estado { get; set; } = "Activo";

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;  // CORREGIDO: Usar UTC

        public DateTime? UltimaActualizacion { get; set; }

        public bool Activo { get; set; } = true;

     // Navegación
     public virtual ICollection<Lote>? Lotes { get; set; }
    }
}
