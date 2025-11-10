using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace checkpoint_web.Models
{
 public class Producto
 {
 [Key]
 public Guid Id { get; set; }

 [Required, MaxLength(50)]
 public string Sku { get; set; } = string.Empty;

 [Required, MaxLength(200)]
 public string Nombre { get; set; } = string.Empty;

 [MaxLength(50)]
 public string Unidad { get; set; } = "u";

 public int VidaUtilDias { get; set; }

 [Column(TypeName = "decimal(10,2)")]
 public decimal TempMin { get; set; }

 [Column(TypeName = "decimal(10,2)")]
 public decimal TempMax { get; set; }

 [Column(TypeName = "decimal(18,3)")]
 public decimal StockMinimo { get; set; }

 public bool Activo { get; set; } = true;

 // Navegación
 public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

 // Comportamientos simples de dominio
 public void Activar() => Activo = true;
 public void Desactivar() => Activo = false;
 public void ActualizarDatos(string nombre, string unidad, int vidaUtilDias, decimal tempMin, decimal tempMax)
 {
 Nombre = nombre;
 Unidad = unidad;
 VidaUtilDias = vidaUtilDias;
 TempMin = tempMin;
 TempMax = tempMax;
 }
 }
}
