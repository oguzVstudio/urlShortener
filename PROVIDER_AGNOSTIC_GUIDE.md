# Provider-Agnostic Registration Strategy

## 🎯 New Approach

You can now **reference and use only the database provider you need**. No need to include all providers!

---

## ✅ Usage: Include Only What You Need

### Using PostgreSQL

#### 1. **Add Project Reference**
```xml
<!-- UrlShortener.Host.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\Infrastructure\UrlShortener.Infrastructure.EfCore.Postgres\UrlShortener.Infrastructure.EfCore.Postgres.csproj" />
</ItemGroup>
```

#### 2. **Use in Program.cs**
```csharp
using UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShortenPostgres(builder.Configuration);
```

#### 3. **appsettings.json**
```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Database=mydb;..."
  }
}
```

---

### Using SQL Server

#### 1. **Add Project Reference**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\Infrastructure\UrlShortener.Infrastructure.EfCore.SqlServer\UrlShortener.Infrastructure.EfCore.SqlServer.csproj" />
</ItemGroup>
```

#### 2. **Use in Program.cs**
```csharp
using UrlShortener.Infrastructure.EfCore.SqlServer.Extensions;

builder.Services.AddShortenSqlServer(builder.Configuration);
```

---

### Using MongoDB

#### 1. **Add Project Reference**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\Infrastructure\UrlShortener.Infrastructure.MongoDB\UrlShortener.Infrastructure.MongoDB.csproj" />
</ItemGroup>
```

#### 2. **Use in Program.cs**
```csharp
using UrlShortener.Infrastructure.MongoDB.Extensions;

builder.Services.AddShortenMongoDb(builder.Configuration);
```

---

## 🔥 Main Extension Methods

| Provider | Extension Method | Package Name |
|----------|-----------------|-----------|
| PostgreSQL | `.AddShortenPostgres(configuration)` | `UrlShortener.Infrastructure.EfCore.Postgres` |
| SQL Server | `.AddShortenSqlServer(configuration)` | `UrlShortener.Infrastructure.EfCore.SqlServer` |
| MongoDB | `.AddShortenMongoDb(configuration)` | `UrlShortener.Infrastructure.MongoDB` |

---

## 📋 Configuration Options

### PostgreSQL & SQL Server
- `ConnectionString`: Database connection string
- `UseInMemory`: Use in-memory database (for testing)
- `MigrationAssembly`: Migration assembly name
- `InMemoryDbName`: In-memory database name

### MongoDB
- `ConnectionString`: MongoDB connection string
- `DatabaseName`: Database name

---

## 🎯 Summary

| Feature | Old | New |
|---------|------|------|
| **Dependencies** | All providers | Only used ones |
| **Build Time** | Slow | Fast |
| **Flexibility** | Low | High |
| **Registration** | `AddPersistence()` | `AddShortenPostgres()` / `AddShortenSqlServer()` / `AddShortenMongoDb()` |
| **Project Size** | Large | Small |

---

**Get started by using only the provider you need! 🚀**
```xml
<!-- UrlShortener.Host.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\Infrastructure\UrlShortener.Infrastructure.EfCore.Postgres\UrlShortener.Infrastructure.EfCore.Postgres.csproj" />
</ItemGroup>

