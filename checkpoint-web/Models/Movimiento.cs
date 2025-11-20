using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
    public class Movimiento
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid LoteId { get; set; }
        public Lote? Lote { get; set; }

        public Guid? SedeId { get; set; }

        public Guid? OrigenUbicacionId { get; set; }
        public Ubicacion? OrigenUbicacion { get; set; }

        public Guid? DestinoUbicacionId { get; set; }
        public Ubicacion? DestinoUbicacion { get; set; }

        /// <summary>
        /// Cliente destino para movimientos de salida/venta
        /// </summary>
        public int? ClienteId { get; set; }
        [ForeignKey(nameof(ClienteId))]
        public virtual Cliente? Cliente { get; set; }

        /// <summary>
        /// Tipo: Ingreso, Traslado, Salida, Devolucion, Ajuste, Consumo
        /// </summary>
        [MaxLength(50)]
        public string Tipo { get; set; } = "Ingreso";

        [Column(TypeName = "decimal(18,3)")]
        public decimal Cantidad { get; set; }

        [MaxLength(50)]
        public string Unidad { get; set; } = "u";

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string? UsuarioId { get; set; }

        [MaxLength(500)]
        public string? Motivo { get; set; }

        /// <summary>
        /// Documento de respaldo: Guía despacho, factura, OC, etc.
        /// </summary>
        [MaxLength(100)]
        public string? NumeroDocumento { get; set; }

        /// <summary>
        /// Estado: Pendiente, Aprobado, Rechazado, Completado
        /// </summary>
        [MaxLength(50)]
        public string? Estado { get; set; } = "Completado";

        /// <summary>
        /// Usuario que aprobó el movimiento (para ajustes)
        /// </summary>
        [MaxLength(450)]
        public string? AprobadoPor { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        /// <summary>
        /// Stock antes del movimiento (para auditoría)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? StockAnterior { get; set; }

        /// <summary>
        /// Stock después del movimiento (para auditoría)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? StockPosterior { get; set; }
    }
}
