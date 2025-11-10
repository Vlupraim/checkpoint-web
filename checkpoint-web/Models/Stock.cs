using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
 public class Stock
 {
 [Key]
 public Guid Id { get; set; }

 [Required]
 public Guid LoteId { get; set; }
 public Lote? Lote { get; set; }

 [Required]
 public Guid UbicacionId { get; set; }
 public Ubicacion? Ubicacion { get; set; }

 [Column(TypeName = "decimal(18,3)")]
 public decimal Cantidad { get; set; }

 [MaxLength(50)]
 public string Unidad { get; set; } = "u";

 public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

 public void Incrementar(decimal cantidad)
 {
 Cantidad += cantidad;
 ActualizadoEn = DateTime.UtcNow;
 }

 public void Decrementar(decimal cantidad)
 {
 Cantidad -= cantidad;
 if (Cantidad <0) Cantidad =0;
 ActualizadoEn = DateTime.UtcNow;
 }
 }
}
