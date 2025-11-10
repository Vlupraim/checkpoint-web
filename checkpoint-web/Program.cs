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
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=CheckpointDev;Trusted_Connection=True;";

builder.Services.AddDbContext<CheckpointDbContext>(options =>
    options.UseSqlServer(connectionString));

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    
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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

// Seed DB
// NOTE: Temporarily disabled to allow application startup while debugging seed/migration issues.
// await SeedData.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
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
