# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project files first for better layer caching
COPY *.csproj ./
COPY *.sln* ./

# Restore NuGet packages
RUN dotnet restore

# Copy remaining source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from builder
COPY --from=builder /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Create directories for logs and data
RUN mkdir -p /app/logs /app/data && chown -R appuser:appuser /app/logs /app/data

# Switch to non-root user
USER appuser

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/New_York

# Expose application port
EXPOSE 80

# Set entry point
ENTRYPOINT ["dotnet", "OrbitComp.dll"]