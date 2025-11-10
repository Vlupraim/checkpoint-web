using Microsoft.EntityFrameworkCore;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("[MINIMAL] Starting minimal configuration...");

try
{
    // Parse DATABASE_URL
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (string.IsNullOrEmpty(databaseUrl) || databaseUrl.Contains("railway.internal"))
 {
        databaseUrl = Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL");
    }
    
    if (string.IsNullOrEmpty(databaseUrl))
  {
        throw new Exception("No DATABASE_URL or DATABASE_PUBLIC_URL found!");
    }
    
    Console.WriteLine($"[MINIMAL] Using database URL: {databaseUrl.Substring(0, Math.Min(50, databaseUrl.Length))}...");
    
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    
    var connStr = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    
    Console.WriteLine($"[MINIMAL] Parsed connection: Host={uri.Host}, Port={uri.Port}");
    
    // Add DbContext
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseNpgsql(connStr));
    
    Console.WriteLine("[MINIMAL] DbContext added");
    
    // Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CheckpointDbContext>()
 .AddDefaultTokenProviders();
    
    Console.WriteLine("[MINIMAL] Identity added");
    
    // Add Razor Pages (MINIMAL - no conventions yet)
    builder.Services.AddRazorPages();

    Console.WriteLine("[MINIMAL] Razor Pages added");
    
    Console.WriteLine("[MINIMAL] Building app...");
    var app = builder.Build();
    Console.WriteLine("[MINIMAL] App built successfully!");
    
    // Minimal pipeline
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
 app.UseAuthorization();
  
    // Simple health check
    app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
    
 app.MapRazorPages();
    
    Console.WriteLine("[MINIMAL] Starting app...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[MINIMAL ERROR] Type: {ex.GetType().FullName}");
    Console.WriteLine($"[MINIMAL ERROR] Message: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[MINIMAL ERROR] Inner Type: {ex.InnerException.GetType().FullName}");
   Console.WriteLine($"[MINIMAL ERROR] Inner Message: {ex.InnerException.Message}");
    }
    
    Console.WriteLine($"[MINIMAL ERROR] Stack: {ex.StackTrace}");
    throw;
}
