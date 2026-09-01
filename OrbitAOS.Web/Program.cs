using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Application.Services;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Data Layer ───────────────────────────────────────────────────────────────
// Migrated from net6.0 to net8.0: connection string retrieval unchanged.
// EF Core 6 → EF Core 8: UseSqlServer API is backward-compatible.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// EF Core developer page exception filter (net8.0 compatible)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Identity ─────────────────────────────────────────────────────────────────
// Migrated from ASP.NET Core Identity 6.x to 8.x.
// Breaking change: RequireConfirmedAccount default behavior unchanged.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ── Clean Architecture DI Registrations ──────────────────────────────────────
// Domain → Infrastructure repositories
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// Application → Services
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── HTTP Pipeline ─────────────────────────────────────────────────────────────
// Migrated from Global.asax Application_Start + App_Start/* to Program.cs middleware pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // Migrated from customErrors in web.config to UseExceptionHandler middleware.
    app.UseExceptionHandler("/Home/Error");
    // HSTS: replaces web.config HTTPS redirect settings.
    app.UseHsts();
}

// Migrated from web.config httpsRedirect to UseHttpsRedirection middleware.
app.UseHttpsRedirection();

// Migrated from /Content and /Scripts folders to wwwroot with UseStaticFiles.
app.UseStaticFiles();

// Migrated from RouteConfig.cs RegisterRoutes to UseRouting + MapControllerRoute.
app.UseRouting();

// Migrated from FormsAuthentication / OWIN cookie middleware to ASP.NET Core Identity.
app.UseAuthentication();
app.UseAuthorization();

// Migrated from RouteConfig.cs default route to MapControllerRoute.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages for Identity UI scaffolded pages.
app.MapRazorPages();

app.Run();
