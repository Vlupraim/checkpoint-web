using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
 public class CalidadLiberacion
 {
 [Key]
 public Guid Id { get; set; }

 [Required]
 public Guid LoteId { get; set; }
 public Lote? Lote { get; set; }

 public string? UsuarioId { get; set; }

 public DateTime Fecha { get; set; } = DateTime.UtcNow;

 [MaxLength(50)]
 public string Estado { get; set; } = "Pendiente";

 [MaxLength(1000)]
 public string? Observacion { get; set; }

 [MaxLength(500)]
 public string? EvidenciaUrl { get; set; }
 }
}
