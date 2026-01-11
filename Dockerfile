# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["UrlShortenerApplication.sln", "./"]
COPY ["src/Host/UrlShortener.Host/UrlShortener.Host.csproj", "src/Host/UrlShortener.Host/"]
COPY ["src/Application/UrlShortener.Application/UrlShortener.Application.csproj", "src/Application/UrlShortener.Application/"]
COPY ["src/Domain/UrlShortener.Domain/UrlShortener.Domain.csproj", "src/Domain/UrlShortener.Domain/"]
COPY ["src/Infrastructure/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "src/Infrastructure/UrlShortener.Infrastructure/"]
COPY ["src/Infrastructure/UrlShortener.Infrastructure.Core/UrlShortener.Infrastructure.Core.csproj", "src/Infrastructure/UrlShortener.Infrastructure.Core/"]
COPY ["src/Infrastructure/UrlShortener.Infrastructure.EfCore.Postgres/UrlShortener.Infrastructure.EfCore.Postgres.csproj", "src/Infrastructure/UrlShortener.Infrastructure.EfCore.Postgres/"]
COPY ["src/Infrastructure/UrlShortener.Infrastructure.Masstransit/UrlShortener.Infrastructure.Masstransit.csproj", "src/Infrastructure/UrlShortener.Infrastructure.Masstransit/"]
COPY ["src/Infrastructure/UrlShortener.Infrastructure.Masstransit.InMemory/UrlShortener.Infrastructure.Masstransit.InMemory.csproj", "src/Infrastructure/UrlShortener.Infrastructure.Masstransit.InMemory/"]

# Restore dependencies
RUN dotnet restore "src/Host/UrlShortener.Host/UrlShortener.Host.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/Host/UrlShortener.Host"
RUN dotnet build "UrlShortener.Host.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UrlShortener.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "UrlShortener.Host.dll"]
