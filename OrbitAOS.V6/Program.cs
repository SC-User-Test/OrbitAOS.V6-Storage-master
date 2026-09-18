using Microsoft.AspNetCore.Identity;
using OrbitAOS.Application;
using OrbitAOS.Infrastructure;
using OrbitAOS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure layer (EF Core 8, repositories) ──────────────────────────
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Application layer (business services) ───────────────────────────────────
builder.Services.AddApplicationServices();

// ── ASP.NET Core Identity (net8.0) ──────────────────────────────────────────
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ── MVC with Views ───────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── HTTP request pipeline ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS: 30-day default. Adjust for production as needed.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ── Routing ──────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages for Identity UI scaffolding
app.MapRazorPages();

app.Run();
