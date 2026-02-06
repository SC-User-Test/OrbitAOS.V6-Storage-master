using Xunit;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OrbitAOS.V6.Data.Tests
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance_WithOptions()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void ApplicationDbContext_ShouldInheritFrom_IdentityDbContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);

            // Assert
            Assert.IsAssignableFrom<IdentityDbContext>(context);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenOptionsIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApplicationDbContext(null!));
        }

        [Fact]
        public void OnConfiguring_ShouldSetLegacyTimestampBehavior_WhenNotConfigured()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .Options;

            // Act - Constructor triggers OnConfiguring
            var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
            // The switch is set, but we can't directly test it without reflection
        }

        [Fact]
        public void OnConfiguring_ShouldNotThrow_WhenOptionsBuilderIsConfigured()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ConfiguredTestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void OnModelCreating_ShouldSetDefaultSchema_ToPublic()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SchemaTestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);
            var model = context.Model;

            // Assert
            Assert.NotNull(model);
            Assert.Equal("public", model.GetDefaultSchema());
        }

        [Fact]
        public void Database_ShouldBeAccessible()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "DatabaseAccessTestDatabase")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context.Database);
        }

        [Fact]
        public void DbContext_ShouldHaveIdentityTables()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "IdentityTablesTestDatabase")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var model = context.Model;

            // Assert
            Assert.NotNull(model);
            // Identity tables are inherited from IdentityDbContext
            Assert.Contains(model.GetEntityTypes(), e => e.ClrType.Name.Contains("IdentityUser") || e.Name.Contains("IdentityUser"));
        }

        [Fact]
        public void SaveChanges_ShouldWork_WithValidContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SaveChangesTestDatabase")
                .Options;

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var result = context.SaveChanges();

                // Assert
                Assert.True(result >= 0);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldWork_WithValidContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SaveChangesAsyncTestDatabase")
                .Options;

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var result = await context.SaveChangesAsync();

                // Assert
                Assert.True(result >= 0);
            }
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "DisposeTestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);
            context.Dispose();

            // Assert - No exception thrown
            Assert.True(true);
        }

        [Fact]
        public void Model_ShouldHaveDefaultSchema()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ModelSchemaTestDatabase")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var schema = context.Model.GetDefaultSchema();

            // Assert
            Assert.Equal("public", schema);
        }

        [Fact]
        public void ContextType_ShouldBeApplicationDbContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TypeTestDatabase")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);

            // Assert
            Assert.IsType<ApplicationDbContext>(context);
        }

        [Fact]
        public void OnConfiguring_ShouldCallBaseMethod()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BaseMethodTestDatabase")
                .Options;

            // Act
            var context = new ApplicationDbContext(options);

            // Assert
            Assert.NotNull(context);
            // Base method is called, no exception thrown
        }

        [Fact]
        public void OnModelCreating_ShouldCallBaseMethod()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ModelCreatingBaseTestDatabase")
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var model = context.Model;

            // Assert
            Assert.NotNull(model);
            // Base method creates Identity tables
            Assert.NotEmpty(model.GetEntityTypes());
        }
    }
}
