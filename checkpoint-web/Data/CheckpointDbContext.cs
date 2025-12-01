using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using checkpoint_web.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace checkpoint_web.Data
{
 public class CheckpointDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
 {
 public CheckpointDbContext(DbContextOptions<CheckpointDbContext> options) : base(options) { }

 public DbSet<Producto> Productos { get; set; } = null!;
 public DbSet<Lote> Lotes { get; set; } = null!;
 public DbSet<Sede> Sedes { get; set; } = null!;
 public DbSet<Ubicacion> Ubicaciones { get; set; } = null!;
 public DbSet<Movimiento> Movimientos { get; set; } = null!;
 public DbSet<Stock> Stocks { get; set; } = null!;
 public DbSet<CalidadLiberacion> CalidadLiberaciones { get; set; } = null!;
 public DbSet<AuditLog> AuditLogs { get; set; } = null!;
     
        // Nuevos DbSets
 public DbSet<Tarea> Tareas { get; set; } = null!;
  public DbSet<Cliente> Clientes { get; set; } = null!;
   public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<Procedimiento> Procedimientos { get; set; } = null!;
public DbSet<Parametro> Parametros { get; set; } = null!;
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;
    public DbSet<TareaComentario> TareaComentarios { get; set; } = null!;

    // DataProtection keys
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

 protected override void OnModelCreating(ModelBuilder builder)
 {
 base.OnModelCreating(builder);

            // Configurar esquema publico para PostgreSQL
   builder.HasDefaultSchema("public");

   // Indices utiles
     builder.Entity<Producto>().HasIndex(p => p.Sku).IsUnique(false);
 builder.Entity<Lote>().HasIndex(l => l.CodigoLote);
    builder.Entity<Lote>().HasIndex(l => l.FechaVencimiento);
        builder.Entity<Ubicacion>().HasIndex(u => new { u.SedeId, u.Codigo });
     builder.Entity<AuditLog>().HasIndex(a => a.UserId);
  builder.Entity<Tarea>().HasIndex(t => t.Estado);
 builder.Entity<Tarea>().HasIndex(t => t.ResponsableId);
       builder.Entity<Tarea>().HasIndex(t => t.FechaLimite);
builder.Entity<Cliente>().HasIndex(c => c.IdentificadorFiscal);
  builder.Entity<Proveedor>().HasIndex(p => p.IdentificadorFiscal);
     builder.Entity<Procedimiento>().HasIndex(p => p.Codigo).IsUnique();
builder.Entity<Parametro>().HasIndex(p => p.Clave).IsUnique();
         builder.Entity<Notificacion>().HasIndex(n => n.UsuarioId);
    builder.Entity<Notificacion>().HasIndex(n => n.Leida);

      // Relaciones existentes
      builder.Entity<Lote>()
    .HasOne(l => l.Producto)
     .WithMany(p => p.Lotes)
    .HasForeignKey(l => l.ProductoId)
       .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lote>()
   .HasOne(l => l.Proveedor)
.WithMany(p => p.Lotes)
 .HasForeignKey(l => l.ProveedorId)
    .OnDelete(DeleteBehavior.SetNull);

   builder.Entity<Ubicacion>()
   .HasOne(u => u.Sede)
 .WithMany(s => s.Ubicaciones)
  .HasForeignKey(u => u.SedeId)
.OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Movimiento>()
       .HasOne(m => m.Lote)
    .WithMany(l => l.Movimientos)
.HasForeignKey(m => m.LoteId)
 .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Movimiento>()
           .HasOne(m => m.OrigenUbicacion)
.WithMany(u => u.MovimientosOrigen)
   .HasForeignKey(m => m.OrigenUbicacionId)
    .OnDelete(DeleteBehavior.Restrict);

     builder.Entity<Movimiento>()
   .HasOne(m => m.DestinoUbicacion)
        .WithMany(u => u.MovimientosDestino)
 .HasForeignKey(m => m.DestinoUbicacionId)
     .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Movimiento>()
       .HasOne(m => m.Cliente)
 .WithMany(c => c.Movimientos)
     .HasForeignKey(m => m.ClienteId)
    .OnDelete(DeleteBehavior.SetNull);

     builder.Entity<Stock>()
     .HasOne(s => s.Lote)
   .WithMany(l => l.Stocks)
  .HasForeignKey(s => s.LoteId)
    .OnDelete(DeleteBehavior.Cascade);

       builder.Entity<Stock>()
      .HasOne(s => s.Ubicacion)
  .WithMany(u => u.Stocks)
.HasForeignKey(s => s.UbicacionId)
       .OnDelete(DeleteBehavior.Cascade);

   // Nuevas relaciones Tarea
         builder.Entity<Tarea>()
          .HasOne(t => t.Producto)
    .WithMany()
      .HasForeignKey(t => t.ProductoId)
   .OnDelete(DeleteBehavior.SetNull);

     builder.Entity<Tarea>()
      .HasOne(t => t.Lote)
      .WithMany()
   .HasForeignKey(t => t.LoteId)
         .OnDelete(DeleteBehavior.SetNull);

      // Relacion TareaComentario -> Tarea
 builder.Entity<TareaComentario>()
       .HasOne(tc => tc.Tarea)
     .WithMany()
    .HasForeignKey(tc => tc.TareaId)
       .OnDelete(DeleteBehavior.Cascade);

   // Relacion TareaComentario -> Usuario
  builder.Entity<TareaComentario>()
   .HasOne(tc => tc.Usuario)
  .WithMany()
   .HasForeignKey(tc => tc.UsuarioId)
        .OnDelete(DeleteBehavior.Restrict);

         // Precision de decimales por defecto
    builder.Entity<Producto>().Property(p => p.StockMinimo).HasPrecision(18,3);
 builder.Entity<Lote>().Property(l => l.CantidadInicial).HasPrecision(18,3);
   builder.Entity<Lote>().Property(l => l.CantidadDisponible).HasPrecision(18,3);
   builder.Entity<Movimiento>().Property(m => m.Cantidad).HasPrecision(18,3);
      builder.Entity<Movimiento>().Property(m => m.StockAnterior).HasPrecision(18,3);
   builder.Entity<Movimiento>().Property(m => m.StockPosterior).HasPrecision(18,3);
builder.Entity<Stock>().Property(s => s.Cantidad).HasPrecision(18,3);

            // Map enum Estado de Lote a integer (0,1,2,3,4) para compatibilidad con PostgreSQL
            builder.Entity<Lote>().Property(l => l.Estado)
                   .HasConversion<int>()
                   .HasColumnType("integer")
                   .HasColumnName("Estado");
        }
    }
}
