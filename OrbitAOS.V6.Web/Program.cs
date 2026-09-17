using Microsoft.AspNetCore.Identity;
using OrbitAOS.V6.Application;
using OrbitAOS.V6.Infrastructure;
using OrbitAOS.V6.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Register layered services ────────────────────────────────────────────────
// Infrastructure layer: EF Core, Identity core, repositories, unit of work
builder.Services.AddInfrastructureServices(builder.Configuration);

// Application layer: business logic services
builder.Services.AddApplicationServices();

// Web layer: add Identity UI (Razor Pages for login/register/manage)
// AddDefaultIdentity builds on top of the IdentityCore already registered in Infrastructure.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Configure the HTTP request pipeline ─────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // Show detailed EF Core migration errors in development
    app.UseMigrationsEndPoint();
}
else
{
    // Production error handling with HSTS
    app.UseExceptionHandler("/Home/Error");
    // HSTS: 30-day max-age. Adjust for production scenarios.
    // See https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Default MVC route: {controller=Home}/{action=Index}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for Identity UI (login, register, manage)
app.MapRazorPages();

app.Run();
