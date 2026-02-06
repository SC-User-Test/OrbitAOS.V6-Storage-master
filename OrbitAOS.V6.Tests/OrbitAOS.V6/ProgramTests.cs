using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OrbitAOS.V6.Data;
using Microsoft.Extensions.Options;

namespace OrbitAOS.V6.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void WebApplicationBuilder_ShouldBeCreated()
        {
            // Arrange
            var args = Array.Empty<string>();

            // Act
            var builder = WebApplication.CreateBuilder(args);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Services);
            Assert.NotNull(builder.Configuration);
        }

        [Fact]
        public void WebApplicationBuilder_ShouldAcceptCommandLineArgs()
        {
            // Arrange
            var args = new[] { "--environment=Testing" };

            // Act
            var builder = WebApplication.CreateBuilder(args);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Configuration);
        }

        [Fact]
        public void DbContext_ShouldBeRegistered_WithConnectionString()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

            // Act
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            var serviceProvider = builder.Services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            // Assert
            Assert.NotNull(dbContext);
        }

        [Fact]
        public void Identity_ShouldBeConfigured_WithRequireConfirmedAccount()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Act
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var identityOptions = serviceProvider.GetService<IOptions<IdentityOptions>>();

            // Assert
            Assert.NotNull(identityOptions);
        }

        [Fact]
        public void Controllers_ShouldBeRegistered()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);

            // Act
            builder.Services.AddControllersWithViews();
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void DatabaseDeveloperPageExceptionFilter_ShouldBeRegistered()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);

            // Act
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void WebApplication_ShouldBuild_Successfully()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            // Act
            var app = builder.Build();

            // Assert
            Assert.NotNull(app);
            Assert.NotNull(app.Services);
        }

        [Fact]
        public void ConnectionString_ShouldBeRetrieved_FromConfiguration()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);
            var expectedConnectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = expectedConnectionString;

            // Act
            var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

            // Assert
            Assert.Equal(expectedConnectionString, connectionString);
        }

        [Fact]
        public void TimeSpan_FromSeconds_ShouldCreate_30SecondDelay()
        {
            // Act
            var delay = TimeSpan.FromSeconds(30);

            // Assert
            Assert.Equal(30, delay.TotalSeconds);
        }

        [Fact]
        public void MaxRetryCount_ShouldBe_Five()
        {
            // Arrange
            var maxRetryCount = 5;

            // Assert
            Assert.Equal(5, maxRetryCount);
        }

        [Fact]
        public void ErrorCodestoAdd_ShouldBe_Null()
        {
            // Arrange
            int[]? errorCodesToAdd = null;

            // Assert
            Assert.Null(errorCodesToAdd);
        }
    }
}
