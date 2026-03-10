# Multi-stage Dockerfile for OrbitAOS.V6 ASP.NET Core 6.0 Application
# Build stage uses SDK image, runtime stage uses specified base image

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy solution file
COPY OrbitAOS.V6.sln ./

# Copy project file for dependency restoration
COPY OrbitAOS.V6/OrbitAOS.V6.csproj ./OrbitAOS.V6/

# Restore dependencies
RUN dotnet restore "OrbitAOS.V6/OrbitAOS.V6.csproj"

# Copy all source code
COPY OrbitAOS.V6/ ./OrbitAOS.V6/

# Build the application
WORKDIR /src/OrbitAOS.V6
RUN dotnet build "OrbitAOS.V6.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "OrbitAOS.V6.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application from builder stage
COPY --from=builder --chown=appuser:appuser /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC

# Expose application port
EXPOSE 80

# Switch to non-root user
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "OrbitAOS.V6.dll"]