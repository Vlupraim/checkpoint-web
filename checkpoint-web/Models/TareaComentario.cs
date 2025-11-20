using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace checkpoint_web.Models
{
    /// <summary>
    /// Comentarios o notas en tareas para colaboración
    /// </summary>
    public class TareaComentario
    {
     [Key]
   public int Id { get; set; }

[Required]
        public int TareaId { get; set; }

        [ForeignKey(nameof(TareaId))]
    public virtual Tarea? Tarea { get; set; }

  [Required]
  [StringLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

  [ForeignKey(nameof(UsuarioId))]
   public virtual ApplicationUser? Usuario { get; set; }

        [Required]
        [StringLength(2000)]
  public string Contenido { get; set; } = string.Empty;

  public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaEdicion { get; set; }

 public bool Activo { get; set; } = true;
    }
}
