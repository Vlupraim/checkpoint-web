using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Representa un cliente o destinatario de productos
    /// </summary>
  public class Cliente
    {
        [Key]
  public int Id { get; set; }

        [Required]
      [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NombreComercial { get; set; }

        /// <summary>
  /// RUT, NIT, RFC, o identificador fiscal segï¿½n paï¿½s
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
        /// Estado: Activo, Inactivo, Suspendido
    /// </summary>
        [StringLength(20)]
        public string Estado { get; set; } = "Activo";

   [StringLength(1000)]
        public string? Observaciones { get; set; }

public DateTime FechaRegistro { get; set; } = DateTime.Now;

   public DateTime? UltimaActualizacion { get; set; }

      public bool Activo { get; set; } = true;

        // Navegacion
        public virtual ICollection<Movimiento>? Movimientos { get; set; }
    }
}
