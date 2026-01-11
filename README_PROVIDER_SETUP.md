# 🎯 Provider-Agnostic Database Configuration

This project has been restructured so you can **include and use only the database provider you need**.

---

## 🚀 Quick Start

### Step 1: Choose a Provider

- **PostgreSQL** → `AddShortenPostgres()`
- **SQL Server** → `AddShortenSqlServer()`
- **MongoDB** → `AddShortenMongoDb()`

### Step 2: Update Program.cs

**For MongoDB:**
```csharp
using UrlShortener.Infrastructure.MongoDB.Extensions;

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInMemoryMasstransit(builder.Configuration);
builder.Services.AddShortenMongoDb(builder.Configuration);
```

**For PostgreSQL:**
```csharp
using UrlShortener.Infrastructure.EfCore.Postgres.Extensions;

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInMemoryMasstransit(builder.Configuration);
builder.Services.AddShortenPostgres(builder.Configuration);

// Apply migrations
await app.Services.ApplyPostgresDatabaseMigrationsAsync();
```

**For SQL Server:**
```csharp
using UrlShortener.Infrastructure.EfCore.SqlServer.Extensions;

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInMemoryMasstransit(builder.Configuration);
builder.Services.AddShortenSqlServer(builder.Configuration);

// Apply migrations
await app.Services.ApplySqlServerDatabaseMigrationsAsync();
```

### Step 3: Configure Settings

**appsettings.json:**
```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Database=mydb;..."
  }
}
```

---

## ✅ Summary

- ✅ **Include only what you need**
- ✅ **Minimal dependencies**
- ✅ **Fast build**
- ✅ **Clean code**

---

## 🔧 Configuration Options

Advanced configuration options for each provider:

### PostgreSQL & SQL Server
- `ConnectionString`: Database connection string
- `UseInMemory`: Use in-memory database (for testing)
- `MigrationAssembly`: Migration assembly name
- `InMemoryDbName`: In-memory database name

### MongoDB
- `ConnectionString`: MongoDB connection string
- `DatabaseName`: Database name

---

**Ready to go? Start using only the provider you need in your project! 🚀**
