# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies to optimize layer caching
COPY ["OrbitAOS.V6.csproj", "./"]
RUN dotnet restore "OrbitAOS.V6.csproj"

# Copy the rest of the source code and build
COPY . .
RUN dotnet build "OrbitAOS.V6.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "OrbitAOS.V6.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
WORKDIR /app

# Create a non-root user for security
RUN adduser -u 1000 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published files from build stage
COPY --from=build /app/publish .

# .NET Environment Variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose the application port
EXPOSE 80

ENTRYPOINT ["dotnet", "OrbitAOS.V6.dll"]
