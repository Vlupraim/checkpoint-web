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

// ============================================
// Database configuration: SOLO PostgreSQL
// ============================================
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
    // Local development - SOLO PostgreSQL desde appsettings.json
    var connectionString = configuration.GetConnectionString("DefaultConnection")
   ?? "Host=localhost;Port=5432;Database=CheckpointDev;Username=postgres;Password=postgres;";
    
    Console.WriteLine($"[STARTUP] Using local PostgreSQL connection");
    Console.WriteLine($"[STARTUP] Connection: {connectionString.Split(';')[0]}"); // Solo mostrar el host

    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseNpgsql(connectionString));
}

Console.WriteLine("[STARTUP] Configuring Identity...");
try
{
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<CheckpointDbContext>()
    .AddDefaultTokenProviders();

    Console.WriteLine("[STARTUP] Identity configured successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"[STARTUP ERROR] Identity configuration failed: {ex.GetType().Name}");
    Console.WriteLine($"[STARTUP ERROR] Message: {ex.Message}");
    if (ex.InnerException != null)
    {
 Console.WriteLine($"[STARTUP ERROR] Inner: {ex.InnerException.Message}");
    }
    throw;
}

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
  options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
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

Console.WriteLine("[STARTUP] Building application...");
var app = builder.Build();

Console.WriteLine("[STARTUP] Application built successfully");

// ============================================
// IMPORTANTE: La base de datos se configura manualmente con SQL
// NO usar migraciones de EF Core
// ============================================

if (app.Environment.IsProduction())
{
    Console.WriteLine("[STARTUP] Production environment detected");
    Console.WriteLine("[STARTUP] Testing database connection...");
    
    try
    {
  using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CheckpointDbContext>();
   
        // Solo probar conexión, NO aplicar migraciones
            var canConnect = await db.Database.CanConnectAsync();
       
            if (canConnect)
        {
 Console.WriteLine("[STARTUP] ? Database connection successful!");
    
          // Verificar que las tablas existen
                try
           {
     var userCount = await db.Users.CountAsync();
     Console.WriteLine($"[STARTUP] ? Found {userCount} users in database");
                }
 catch (Exception ex)
      {
          Console.WriteLine($"[STARTUP] ?? Could not query users: {ex.Message}");
            Console.WriteLine("[STARTUP] ?? Please ensure database schema is created using railway-setup-complete.sql");
     }
   }
   else
            {
    Console.WriteLine("[STARTUP] ? Database connection FAILED");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP ERROR] Database test failed: {ex.GetType().Name}");
        Console.WriteLine($"[STARTUP ERROR] Message: {ex.Message}");
   if (ex.InnerException != null)
        {
    Console.WriteLine($"[STARTUP ERROR] Inner: {ex.InnerException.Message}");
        }
        Console.WriteLine("[STARTUP] ?? Application will continue but database may not be accessible");
    }
}
else
{
    Console.WriteLine("[STARTUP] Development environment detected");
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

// Enhanced health check endpoint for Railway
app.MapGet("/health", async (CheckpointDbContext db) =>
{
    try
    {
        // Test database connection
        var canConnect = await db.Database.CanConnectAsync();
   
        if (!canConnect)
{
            return Results.Json(new
      {
      status = "unhealthy",
            timestamp = DateTime.UtcNow,
                error = "Database connection failed"
            }, statusCode: 503);
        }

        // Check if users table exists and has data
        var userCount = await db.Users.CountAsync();
  
        return Results.Ok(new
        {
      status = "healthy",
 timestamp = DateTime.UtcNow,
    database = new
    {
            connected = true,
                users = userCount,
       provider = "PostgreSQL"
            }
     });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
   status = "unhealthy",
            timestamp = DateTime.UtcNow,
     error = ex.Message,
errorType = ex.GetType().Name
        }, statusCode: 503);
    }
})
.AllowAnonymous();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("[STARTUP] Application started successfully!");
Console.WriteLine("[STARTUP] Ready to accept requests");

app.Run();
