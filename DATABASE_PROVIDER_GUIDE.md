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
    "ConnectionString": "Server=localhost;Port=5432;Database=mydb;Username=user;Password=pass",
    "DefaultSchema": "shorten",
    "TablePrefix": "",
    "MigrationDefaultSchema": "public",
    "MigrationAssembly": null
  }
}
```

**SQL Server:**
```json
{
  "SqlServerOptions": {
    "ConnectionString": "Server=localhost;Database=mydb;User Id=sa;Password=pass;",
    "DefaultSchema": "shorten",
    "TablePrefix": "",
    "MigrationDefaultSchema": "dbo",
    "MigrationAssembly": null
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

## ⚙️ Configuration Options

### PostgreSQL Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | string | required | PostgreSQL connection string |
| `DefaultSchema` | string | `"shorten"` | Default schema for tables |
| `TablePrefix` | string | `""` | Prefix to add before table names (e.g., `"app"` → `app_shorten_urls`) |
| `MigrationDefaultSchema` | string | `"public"` | Schema for migration history table |
| `MigrationAssembly` | string | `null` | Assembly name for migrations (auto-detected if null) |
| `UseInMemory` | bool | `false` | Use in-memory database for testing |

**Example with table prefix:**
```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Port=5432;Database=mydb;Username=user;Password=pass",
    "TablePrefix": "app"
  }
}
```
**Result:** Tables will be named `app_shorten_urls` and `app_short_url_tracks` in the `shorten` schema.

### SQL Server Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | string | required | SQL Server connection string |
| `DefaultSchema` | string | `"shorten"` | Default schema for tables |
| `TablePrefix` | string | `""` | Prefix to add before table names (e.g., `"App"` → `AppShortenUrls`) |
| `MigrationDefaultSchema` | string | `"dbo"` | Schema for migration history table |
| `MigrationAssembly` | string | `null` | Assembly name for migrations (auto-detected if null) |
| `UseInMemory` | bool | `false` | Use in-memory database for testing |

**Example with table prefix:**
```json
{
  "SqlServerOptions": {
    "ConnectionString": "Server=localhost;Database=mydb;User Id=sa;Password=pass;",
    "TablePrefix": "App"
  }
}
```
**Result:** Tables will be named `AppShortenUrls` and `AppShortUrlTracks` in the `shorten` schema.

### MongoDB Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | string | required | MongoDB connection string |
| `DatabaseName` | string | required | Database name |

---

## ✅ Advantages

- ✅ **Include only what you need** - No unnecessary dependencies
- ✅ **Clean code** - Each provider is independent
- ✅ **Fast build** - Fewer projects
- ✅ **Easy switching** - Simple to change providers

---

**For detailed information:** [PROVIDER_AGNOSTIC_GUIDE.md](PROVIDER_AGNOSTIC_GUIDE.md)
