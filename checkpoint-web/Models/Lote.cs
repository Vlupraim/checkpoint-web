using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace checkpoint_web.Models
{
    public class Lote
    {
 [Key]
 public Guid Id { get; set; }

 [Required]
        public Guid ProductoId { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Proveedor que suministró este lote
      /// </summary>
   public int? ProveedorId { get; set; }
   [ForeignKey(nameof(ProveedorId))]
    public virtual Proveedor? Proveedor { get; set; }

 [MaxLength(100)]
    public string CodigoLote { get; set; } = string.Empty;

  public DateTime FechaIngreso { get; set; }
    public DateTime? FechaVencimiento { get; set; }

        [MaxLength(100)]
        public string? OrdenCompra { get; set; }

 [MaxLength(100)]
        public string? GuiaRecepcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
   public decimal TempIngreso { get; set; }

        /// <summary>
        /// Estado del lote en el flujo de control de calidad.
        /// Inicial: Cuarentena (esperando revisión)
        /// Solo lotes LIBERADOS pueden usarse en operaciones.
        /// </summary>
        public EstadoLote Estado { get; set; } = EstadoLote.Cuarentena;

   [Column(TypeName = "decimal(18,3)")]
        public decimal CantidadInicial { get; set; }

        [Column(TypeName = "decimal(18,3)")]
public decimal CantidadDisponible { get; set; }

        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
