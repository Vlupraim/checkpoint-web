using Microsoft.EntityFrameworkCore;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using checkpoint_web.Services;
using checkpoint_web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Early logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
var configuration = builder.Configuration;

// Database configuration: Railway (PostgreSQL) or Local (SQL Server)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

Console.WriteLine($"[STARTUP] Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"[STARTUP] DATABASE_URL present: {!string.IsNullOrEmpty(databaseUrl)}");

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Railway PostgreSQL
    Console.WriteLine("[STARTUP] Parsing Railway DATABASE_URL...");
     var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        var host = databaseUri.Host;
        var database = databaseUri.AbsolutePath.Trim('/');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
  var port = databaseUri.Port;
 
   var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        
    Console.WriteLine($"[STARTUP] PostgreSQL connection configured: Host={host}, Port={port}, Database={database}");
    
        builder.Services.AddDbContext<CheckpointDbContext>(options =>
    options.UseNpgsql(connectionString));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP ERROR] Failed to parse DATABASE_URL: {ex.Message}");
        throw;
    }
}
else
{
    // Local development - SQL Server or PostgreSQL from appsettings.json
 var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=CheckpointDev;Trusted_Connection=True;";
    
    Console.WriteLine($"[STARTUP] Using local connection string");

    // Detect if it's PostgreSQL or SQL Server based on connection string
    if (connectionString.Contains("Host=") || connectionString.Contains("host="))
    {
        Console.WriteLine("[STARTUP] Detected PostgreSQL from connection string");
        builder.Services.AddDbContext<CheckpointDbContext>(options =>
     options.UseNpgsql(connectionString));
    }
    else
    {
        Console.WriteLine("[STARTUP] Detected SQL Server from connection string");
        builder.Services.AddDbContext<CheckpointDbContext>(options =>
    options.UseSqlServer(connectionString));
    }
}

Console.WriteLine("[STARTUP] Configuring Identity...");
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
})
    .AddEntityFrameworkStores<CheckpointDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie behavior: TRUE session-only cookies
builder.Services.ConfigureApplicationCookie(options =>
{
 options.Cookie.Name = "Checkpoint.Auth";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
 options.Cookie.Path = "/";
  options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Changed for Railway compatibility
    
    // CRITICAL: Do NOT set ExpireTimeSpan - this creates persistent cookies
 // The cookie will expire when browser closes (true session cookie)
    options.SlidingExpiration = false;
    options.Cookie.MaxAge = null;
    options.Cookie.Expiration = null;
    
    // Override cookie creation to force session cookies
    options.Events = new CookieAuthenticationEvents
 {
        OnSigningIn = context =>
        {
         // Always force session cookie (no expiration)
   context.Properties.IsPersistent = false;
        context.Properties.ExpiresUtc = null;
 
        // Force cookie options to be session-only
       context.CookieOptions.Expires = null;
            context.CookieOptions.MaxAge = null;
         
return Task.CompletedTask;
        },
  OnValidatePrincipal = async context =>
        {
   // Validate that cookie is still a session cookie
     if (context.Properties.IsPersistent)
     {
           // Force it back to session-only
         context.Properties.IsPersistent = false;
   context.Properties.ExpiresUtc = null;
  context.ShouldRenew = true;
   }
   await Task.CompletedTask;
        }
    };
});

// Configure Antiforgery cookies to be session-only
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "CheckpointAntiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.MaxAge = null;
  options.Cookie.Expiration = null;
    options.Cookie.IsEssential = true;
});

// Require authenticated users by default. AllowAnonymous on specific pages will override.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
    .Build();
});

builder.Services.AddRazorPages(options =>
{
    // Allow anonymous access to login/logout and public pages
  options.Conventions.AllowAnonymousToPage("/Account/Login");
  options.Conventions.AllowAnonymousToPage("/Account/Logout");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Account/CookieDiagnostics");
});

builder.Services.AddControllersWithViews();

// Audit service
builder.Services.AddScoped<IAuditService, AuditService>();
// Domain services
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<ISedeService, SedeService>();
builder.Services.AddScoped<IUbicacionService, UbicacionService>();
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<ICalidadService, CalidadService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

var app = builder.Build();

Console.WriteLine("[STARTUP] Application built successfully");

// Apply migrations and seed data in production
if (app.Environment.IsProduction())
{
 Console.WriteLine("[STARTUP] Production environment detected, applying migrations...");
    using (var scope = app.Services.CreateScope())
    {
      try
{
   var db = scope.ServiceProvider.GetRequiredService<CheckpointDbContext>();
 Console.WriteLine("[STARTUP] DbContext acquired, checking database connection...");
     
 // Test connection
          await db.Database.CanConnectAsync();
   Console.WriteLine("[STARTUP] Database connection successful");
            
   Console.WriteLine("[STARTUP] Applying migrations...");
   await db.Database.MigrateAsync();
    Console.WriteLine("[STARTUP] Migrations applied successfully");
  
         Console.WriteLine("[STARTUP] Seeding database...");
      await SeedData.InitializeAsync(scope.ServiceProvider);
       Console.WriteLine("[STARTUP] Database seeded successfully");
        }
        catch (Exception ex)
  {
      Console.WriteLine($"[STARTUP ERROR] Database initialization failed: {ex.GetType().Name}");
            Console.WriteLine($"[STARTUP ERROR] Message: {ex.Message}");
      if (ex.InnerException != null)
            {
    Console.WriteLine($"[STARTUP ERROR] Inner: {ex.InnerException.Message}");
   }
  Console.WriteLine("[STARTUP] Application will continue without database initialization");
      }
    }
}
else
{
    Console.WriteLine("[STARTUP] Development environment, skipping automatic migrations");
}

Console.WriteLine("[STARTUP] Configuring HTTP pipeline...");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}

// Only use HTTPS redirection in production with proper certificate
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Audit middleware (after authentication so we can capture user)
app.UseMiddleware<AuditMiddleware>();

// Redirect root '/' to login page for anonymous users only
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
 {
        var user = context.User;
 if (!(user?.Identity?.IsAuthenticated ?? false))
  {
     if (!context.Request.Path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase))
          {
       context.Response.Redirect("/Account/Login");
      return;
 }
     }
    }
    await next();
});

app.UseAuthorization();

// Simple health check endpoint for Railway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
