using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace checkpoint_web.Models
{
 public class Sede
 {
 [Key]
 public Guid Id { get; set; }

 [Required, MaxLength(150)]
 public string Nombre { get; set; } = string.Empty;

 [MaxLength(50)]
 public string Codigo { get; set; } = string.Empty;

 [MaxLength(250)]
 public string? Direccion { get; set; }

 public bool Activa { get; set; } = true;

 public ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
 }
}
