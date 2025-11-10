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

// Add services to the container.
var configuration = builder.Configuration;

// Database configuration: Railway (PostgreSQL) or Local (SQL Server)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway PostgreSQL
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var host = databaseUri.Host;
 var database = databaseUri.AbsolutePath.Trim('/');
    var username = userInfo[0];
    var password = userInfo[1];
 var port = databaseUri.Port;
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Local development - SQL Server or PostgreSQL from appsettings.json
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=CheckpointDev;Trusted_Connection=True;";
    
    // Detect if it's PostgreSQL or SQL Server based on connection string
    if (connectionString.Contains("Host=") || connectionString.Contains("host="))
    {
  builder.Services.AddDbContext<CheckpointDbContext>(options =>
     options.UseNpgsql(connectionString));
    }
    else
    {
  builder.Services.AddDbContext<CheckpointDbContext>(options =>
    options.UseSqlServer(connectionString));
    }
}

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

// Apply migrations and seed data in production
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
    try
        {
   var db = scope.ServiceProvider.GetRequiredService<CheckpointDbContext>();
            await db.Database.MigrateAsync();
       await SeedData.InitializeAsync(scope.ServiceProvider);
  }
 catch (Exception ex)
  {
      var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
     logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}

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

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
