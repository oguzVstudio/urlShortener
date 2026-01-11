# Database Provider Registration Strategy

## ✨ Simplified Approach

Your project has a **provider-agnostic** architecture. Just include the database provider you want to use!

---

## 🚀 How to Use?

### 1. Choose a Provider and Add Reference

**For PostgreSQL:**
```xml
<ProjectReference Include="...\UrlShortener.Infrastructure.EfCore.Postgres\..." />
```

**For SQL Server:**
```xml
<ProjectReference Include="...\UrlShortener.Infrastructure.EfCore.SqlServer\..." />
```

**For MongoDB:**
```xml
<ProjectReference Include="...\UrlShortener.Infrastructure.MongoDB\..." />
```

---

### 2. Register in Program.cs

```csharp
// Core infrastructure (domain, application, cache, distributed lock)
builder.Services.AddInfrastructure(builder.Configuration);

// Messaging provider
builder.Services.AddInMemoryMasstransit(builder.Configuration);

// Add only the provider you'll use:
builder.Services.AddShortenPostgres(builder.Configuration);    // PostgreSQL
// or
builder.Services.AddShortenSqlServer(builder.Configuration);   // SQL Server
// or
builder.Services.AddShortenMongoDb(builder.Configuration);     // MongoDB
```

---

### 3. Configure in appsettings.json

**PostgreSQL:**
```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Port=5432;Database=mydb;Username=user;Password=pass"
  }
}
```

**SQL Server:**
```json
{
  "SqlServerOptions": {
    "ConnectionString": "Server=localhost;Database=mydb;User Id=sa;Password=pass;"
  }
}
```

**MongoDB:**
```json
{
  "MongoDbOptions": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "mydb"
  }
}
```

---

## 📦 Provider Extension Methods

| Provider | Extension Method | Namespace |
|----------|-----------------|-----------|
| PostgreSQL | `AddShortenPostgres(configuration)` | `UrlShortener.Infrastructure.EfCore.Postgres.Extensions` |
| SQL Server | `AddShortenSqlServer(configuration)` | `UrlShortener.Infrastructure.EfCore.SqlServer.Extensions` |
| MongoDB | `AddShortenMongoDb(configuration)` | `UrlShortener.Infrastructure.MongoDB.Extensions` |

---

## 📋 Migrations

### PostgreSQL
```csharp
await app.Services.ApplyPostgresDatabaseMigrationsAsync();
```

### SQL Server
```csharp
await app.Services.ApplySqlServerDatabaseMigrationsAsync();
```

### MongoDB
No migrations needed (schema-less).

---

## ✅ Advantages

- ✅ **Include only what you need** - No unnecessary dependencies
- ✅ **Clean code** - Each provider is independent
- ✅ **Fast build** - Fewer projects
- ✅ **Easy switching** - Simple to change providers

---

**For detailed information:** [PROVIDER_AGNOSTIC_GUIDE.md](PROVIDER_AGNOSTIC_GUIDE.md)
