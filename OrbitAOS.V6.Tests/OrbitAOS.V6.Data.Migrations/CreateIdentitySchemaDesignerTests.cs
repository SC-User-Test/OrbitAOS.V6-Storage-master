using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OrbitAOS.V6.Data;
using OrbitAOS.V6.Data.Migrations;
using System.Reflection;

namespace OrbitAOS.V6.Data.Migrations.Tests
{
    public class CreateIdentitySchemaDesignerTests
    {
        [Fact]
        public void CreateIdentitySchema_ShouldHave_DbContextAttribute()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var attributes = migrationType.GetCustomAttributes(typeof(DbContextAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);
            var attribute = attributes[0] as DbContextAttribute;
            Assert.NotNull(attribute);
            Assert.Equal(typeof(ApplicationDbContext), attribute.ContextType);
        }

        [Fact]
        public void CreateIdentitySchema_ShouldHave_MigrationAttribute()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var attributes = migrationType.GetCustomAttributes(typeof(MigrationAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);
            var attribute = attributes[0] as MigrationAttribute;
            Assert.NotNull(attribute);
            Assert.Equal("00000000000000_CreateIdentitySchema", attribute.Id);
        }

        [Fact]
        public void BuildTargetModel_ShouldExist_InPartialClass()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var methodInfo = migrationType.GetMethod("BuildTargetModel",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
        }

        [Fact]
        public void BuildTargetModel_ShouldBe_Protected()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var methodInfo = migrationType.GetMethod("BuildTargetModel",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
            Assert.True(methodInfo.IsFamily);
        }

        [Fact]
        public void BuildTargetModel_ShouldAccept_ModelBuilderParameter()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var methodInfo = migrationType.GetMethod("BuildTargetModel",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = methodInfo?.GetParameters();

            // Assert
            Assert.NotNull(parameters);
            Assert.Single(parameters);
            Assert.Equal("ModelBuilder", parameters[0].ParameterType.Name);
        }

        [Fact]
        public void BuildTargetModel_ReturnType_ShouldBe_Void()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var methodInfo = migrationType.GetMethod("BuildTargetModel",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
            Assert.Equal(typeof(void), methodInfo.ReturnType);
        }

        [Fact]
        public void CreateIdentitySchema_ShouldBe_PartialClass()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Assert
            Assert.True(migrationType.IsClass);
            Assert.False(migrationType.IsAbstract);
            Assert.False(migrationType.IsSealed);
        }

        [Fact]
        public void Migration_ShouldInherit_FromMigrationClass()
        {
            // Arrange
            var migration = new CreateIdentitySchema();

            // Assert
            Assert.IsAssignableFrom<Migration>(migration);
        }

        [Fact]
        public void MigrationId_ShouldBe_00000000000000_CreateIdentitySchema()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);
            var attribute = migrationType.GetCustomAttribute<MigrationAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal("00000000000000_CreateIdentitySchema", attribute.Id);
        }

        [Fact]
        public void DbContextType_ShouldBe_ApplicationDbContext()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);
            var attribute = migrationType.GetCustomAttribute<DbContextAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal(typeof(ApplicationDbContext), attribute.ContextType);
        }

        [Fact]
        public void Namespace_ShouldBe_OrbitAOSV6DataMigrations()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Assert
            Assert.Equal("OrbitAOS.V6.Data.Migrations", migrationType.Namespace);
        }

        [Fact]
        public void Class_ShouldBe_Public()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Assert
            Assert.True(migrationType.IsPublic);
        }

        [Fact]
        public void BuildTargetModel_ShouldHave_OverrideModifier()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);
            var methodInfo = migrationType.GetMethod("BuildTargetModel",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodInfo);
            Assert.True(methodInfo.IsVirtual);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act
            var migration = new CreateIdentitySchema();

            // Assert
            Assert.NotNull(migration);
            Assert.IsType<CreateIdentitySchema>(migration);
        }

        [Fact]
        public void Migration_ShouldHave_AllRequiredMethods()
        {
            // Arrange
            var migrationType = typeof(CreateIdentitySchema);

            // Act
            var upMethod = migrationType.GetMethod("Up", BindingFlags.NonPublic | BindingFlags.Instance);
            var downMethod = migrationType.GetMethod("Down", BindingFlags.NonPublic | BindingFlags.Instance);
            var buildTargetModelMethod = migrationType.GetMethod("BuildTargetModel", BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.NotNull(upMethod);
            Assert.NotNull(downMethod);
            Assert.NotNull(buildTargetModelMethod);
        }
    }
}
