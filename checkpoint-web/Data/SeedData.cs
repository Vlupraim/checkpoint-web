using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using checkpoint_web.Models;

namespace checkpoint_web.Data
{
 public static class SeedData
 {
 public static async Task InitializeAsync(IServiceProvider services)
 {
 using var scope = services.CreateScope();
 var provider = scope.ServiceProvider;
 var context = provider.GetRequiredService<CheckpointDbContext>();
 var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
 var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        // ============================================
            // NO aplicar migraciones - la BD se configura manualmente
   // ============================================
            
    // Test connection only
try
     {
    var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
       {
               Console.WriteLine("[SEED] Cannot connect to database - skipping seed");
    return;
  }
            }
       catch (Exception ex)
      {
       Console.WriteLine($"[SEED] Database connection failed: {ex.Message}");
     return;
     }

 // Roles
 string[] roles = new[] { "Administrador", "PersonalBodega", "ControlCalidad" };
 foreach (var role in roles)
 {
 if (!await roleManager.RoleExistsAsync(role))
 {
 await roleManager.CreateAsync(new IdentityRole(role));
 }
 }

 // Admin user
 var adminEmail = "admin@example.com";
 var admin = await userManager.FindByEmailAsync(adminEmail);
 if (admin == null)
 {
 admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, Nombre = "Admin", Activo = true };
 var result = await userManager.CreateAsync(admin, "Admin123!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(admin, "Administrador");
 }
 }

 // Bodega user
 var bodegaEmail = "bodega@example.com";
 var bodega = await userManager.FindByEmailAsync(bodegaEmail);
 if (bodega == null)
 {
 bodega = new ApplicationUser { UserName = bodegaEmail, Email = bodegaEmail, Nombre = "Personal Bodega", Activo = true };
 var result = await userManager.CreateAsync(bodega, "Bodega123!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(bodega, "PersonalBodega");
 }
 }

 // Calidad user
 var calidadEmail = "calidad@example.com";
 var calidad = await userManager.FindByEmailAsync(calidadEmail);
 if (calidad == null)
 {
 calidad = new ApplicationUser { UserName = calidadEmail, Email = calidadEmail, Nombre = "Control Calidad", Activo = true };
 var result = await userManager.CreateAsync(calidad, "Calidad123!");
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(calidad, "ControlCalidad");
 }
 }

 // Sample Productos (solo si la tabla está vacía)
 if (!context.Productos.Any())
 {
 context.Productos.AddRange(new Producto
 {
 Id = Guid.NewGuid(),
 Sku = "PRD-001",
 Nombre = "Producto de ejemplo1",
 Unidad = "u",
 VidaUtilDias =365,
 TempMin =2m,
 TempMax =8m,
 StockMinimo =10m,
 Activo = true
 }, new Producto
 {
 Id = Guid.NewGuid(),
 Sku = "PRD-002",
 Nombre = "Producto de ejemplo2",
 Unidad = "kg",
 VidaUtilDias =180,
 TempMin =0m,
 TempMax =20m,
 StockMinimo =5m,
 Activo = true
 });
 await context.SaveChangesAsync();
 }
 }
 }
}
