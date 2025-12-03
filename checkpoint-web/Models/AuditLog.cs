using System;
using System.ComponentModel.DataAnnotations;

namespace checkpoint_web.Models
{
 public class AuditLog
 {
 [Key]
 public Guid Id { get; set; }

 [MaxLength(256)]
 public string? UserId { get; set; } = string.Empty; // ? NULLABLE ahora

 [MaxLength(200)]
 public string Action { get; set; } = string.Empty;

 [MaxLength(2000)]
 public string? Details { get; set; }

 public DateTime Timestamp { get; set; } = DateTime.UtcNow;
 }
}
