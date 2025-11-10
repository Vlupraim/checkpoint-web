using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
 public class Ubicacion
 {
 [Key]
 public Guid Id { get; set; }

 [Required]
 public Guid SedeId { get; set; }
 public Sede? Sede { get; set; }

 [Required, MaxLength(50)]
 public string Codigo { get; set; } = string.Empty;

 [MaxLength(100)]
 public string Tipo { get; set; } = "General";

 [Column(TypeName = "decimal(18,3)")]
 public decimal Capacidad { get; set; }

 public ICollection<Movimiento> MovimientosOrigen { get; set; } = new List<Movimiento>();
 public ICollection<Movimiento> MovimientosDestino { get; set; } = new List<Movimiento>();
 public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
 }
}
