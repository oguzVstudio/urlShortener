# URL Shortener Application

A modern, high-performance URL shortening service built with .NET 10, featuring Clean Architecture, distributed caching, rate limiting, and asynchronous URL tracking.

## 🚀 Features

- ✨ URL shortening with automatic code generation
- 🔄 Automatic redirection to original URLs
- ⏱️ Optional URL expiration
- 📊 Access tracking and analytics
- 🚀 Redis-based distributed caching with HybridCache
- 🔒 Distributed locking for unique code generation
- ⚡ Rate limiting to prevent abuse
- 📨 Event-driven architecture with MassTransit
- 🗄️ Multi-database support (PostgreSQL, SQL Server, MongoDB)

## 🛠️ Tech Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Core Minimal APIs** - Lightweight and performant API structure
- **Entity Framework Core 10** - ORM layer
- **Redis** - Distributed cache and locking
- **MassTransit** - Message bus (InMemory/RabbitMQ)
- **HybridCache** - Multi-level caching
- **Docker & Docker Compose** - Container support

## 📐 Architecture

The project follows Clean Architecture principles with layered separation:

```
├── Domain                    # Domain entities and interfaces
├── Application              # Business logic and services
├── Infrastructure           # External services and implementations
│   ├── Core                # Core infrastructure utilities
│   ├── EfCore.Postgres     # PostgreSQL implementation
│   ├── EfCore.SqlServer    # SQL Server implementation
│   ├── MongoDB             # MongoDB implementation
│   ├── Masstransit         # MassTransit core
│   ├── Masstransit.InMemory    # InMemory message bus
│   └── Masstransit.RabbitMq    # RabbitMQ message bus
└── Host                     # API endpoints and configuration
```

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) (recommended)
- [PostgreSQL 16+](https://www.postgresql.org/download/) (optional)
- [Redis 7+](https://redis.io/download) (optional)

## ⚙️ Configuration

### Provider-Agnostic Architecture

The project supports multiple database providers. Choose and register only what you need:

**Program.cs:**
```csharp
// Core infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Messaging provider (choose one)
builder.Services.AddInMemoryMasstransit(builder.Configuration);
// builder.Services.AddRabbitMqMasstransit(builder.Configuration);

// Database provider (choose one)
builder.Services.AddShortenPostgres(builder.Configuration);
// builder.Services.AddShortenSqlServer(builder.Configuration);
// builder.Services.AddShortenMongoDb(builder.Configuration);
```

**appsettings.json:**
```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Port=5432;Database=shorten_new;Username=postgres;Password=123456"
  },
  "RedisOptions": {
    "Host": "localhost",
    "Port": 6379,
    "Password": "redis123"
  },
  "ShortenUrlSettings": {
    "BaseUrl": "http://localhost:5028"
  }
}
```

For detailed provider configuration, see [DATABASE_PROVIDER_GUIDE.md](DATABASE_PROVIDER_GUIDE.md).

## 🎯 Getting Started

### Method 1: Run with Docker (Recommended)

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd urlShortener
   ```
2. **Start with Docker Compose**

   ```bash
   docker-compose up -d
   ```

   This command starts:

   - ✅ PostgreSQL database (port 5432)
   - ✅ Redis cache (port 6379)
   - ✅ URL Shortener API (port 8080)
3. **Database migrations are applied automatically**

   The application automatically applies migrations on startup for EF Core providers (PostgreSQL/SQL Server).

   ```csharp
   // In Program.cs (for PostgreSQL)
   await app.Services.ApplyPostgresDatabaseMigrationsAsync();
   
   // Or for SQL Server
   await app.Services.ApplySqlServerDatabaseMigrationsAsync();
   ```

   MongoDB does not require migrations (schema-less).

API is ready at `http://localhost:8080`!

**To stop the application:**

```bash
docker-compose down
```

**To clean up volumes as well:**

```bash
docker-compose down -v
```

### Method 2: Local Development Environment

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd urlShortener
   ```
2. **Start required services**

   ```bash
   # PostgreSQL
   docker run -d --name postgres -p 5432:5432 \
     -e POSTGRES_PASSWORD=123456 \
     -e POSTGRES_DB=shorten_new \
     postgres:16-alpine

   # Redis
   docker run -d --name redis -p 6379:6379 \
     redis:7-alpine redis-server --requirepass redis123
   ```
3. **Apply migrations** (automatic on startup)

   Migrations are applied automatically when the application starts.
   
   To manually apply migrations:

   ```bash
   # PostgreSQL
   dotnet ef database update \
     --project src/Infrastructure/UrlShortener.Infrastructure.EfCore.Postgres \
     --startup-project src/Host/UrlShortener.Host
   
   # SQL Server
   dotnet ef database update \
     --project src/Infrastructure/UrlShortener.Infrastructure.EfCore.SqlServer \
     --startup-project src/Host/UrlShortener.Host
   ```
4. **Run the application**

   ```bash
   dotnet run --project src/Host/UrlShortener.Host/UrlShortener.Host.csproj
   ```

The API will be running at `http://localhost:5028`.

## 📡 API Endpoints

### 1. Shorten URL

Converts a long URL into a shortened URL.

**Endpoint:** `POST /api/urls/shorten`

**Request Body:**

```json
{
  "url": "https://www.example.com/very/long/url/path",
  "isExpiring": false,
  "expiresAt": null
}
```

**Response:** `200 OK`

```json
{
  "shortUrl": "http://localhost:8080/abc123",
  "alias": "abc123",
  "success": true
}
```

**Example (Docker):**

```bash
curl -X POST http://localhost:8080/api/urls/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://www.example.com/very/long/url",
    "isExpiring": false
  }'
```

**Example (Local):**

```bash
curl -X POST http://localhost:5028/api/urls/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://www.example.com/very/long/url",
    "isExpiring": true,
    "expiresAt": "2026-12-31T23:59:59Z"
  }'
```

### 2. Get Original URL

Retrieves the original URL for a given short code (without redirecting).

**Endpoint:** `GET /api/urls/{code}`

**Parameters:**

- `code` (path) - Short URL code

**Response:** `200 OK`

```json
{
  "originalUrl": "https://www.example.com/very/long/url/path",
  "found": true
}
```

**Response (Not Found):**

```json
{
  "originalUrl": null,
  "found": false
}
```

**Rate Limit:** 5 requests per 10 seconds

**Example:**

```bash
# Docker
curl http://localhost:8080/api/urls/abc123

# Local
curl http://localhost:5028/api/urls/abc123
```

### 3. Redirect to Original URL

Redirects to the original URL using the short code and asynchronously tracks the access.

**Endpoint:** `GET /{code}`

**Parameters:**

- `code` (path) - Short URL code

**Response:** `302 Redirect` (to original URL)

**Rate Limit:** 5 requests per 10 seconds

**Example:**

```bash
# Docker - with redirect following
curl -L http://localhost:8080/abc123

# Local - with redirect following
curl -L http://localhost:5028/abc123

# To see only redirect headers
curl -I http://localhost:8080/abc123
```

**Tracked Information:**

- 🌐 IP Address
- 🖥️ User Agent
- ⏰ Access Timestamp

## 🔐 Code Generation Algorithm

The service automatically generates unique 7-character codes:

- **Character Set:** A-Z, a-z, 0-9 (62 characters)
- **Code Length:** 7 characters (default)
- **Collision Control:** Using Redis distributed lock
- **Total Combinations:** 62^7 ≈ 3.5 trillion possible codes
- **Safety:** Thread-safe and distributed-safe code generation

**Algorithm Flow:**

1. Generate random 7-character code
2. Acquire Redis distributed lock
3. Check code existence in database
4. Save if unique, otherwise generate new code
5. Release lock

## 💾 Caching Strategy

The application uses a two-level hybrid caching approach:

### 1. Local Cache (L1)

- In-memory cache
- Ultra-fast access
- 1 minute expiration

### 2. Distributed Cache (L2)

- Redis distributed cache
- Consistency across all instances
- 5 minute expiration

### Benefits

- ⚡ Microsecond-level speed with local cache
- 🔄 Data consistency with distributed cache
- 📉 90%+ reduction in database load
- 🚀 High scalability

## ⚡ Rate Limiting

Fixed window rate limiting is applied to endpoints:

- **Limit:** 5 requests per 10 seconds (per client)
- **Strategy:** Fixed Window
- **Response:** HTTP 429 Too Many Requests
- **Affected Endpoints:**
  - `GET /{code}` (Redirect)
  - `GET /api/urls/{code}` (Lookup)

**Rate limit exceeded example:**

```json
{
  "status": 429,
  "title": "Too Many Requests",
  "detail": "Rate limit exceeded. Try again later."
}
```

## 🗄️ Database Schema

### Table: shorten_urls

Stores main URL information.

| Column             | Type          | Description                 |
| ------------------ | ------------- | --------------------------- |
| `id`             | uuid          | Primary key                 |
| `long_url`       | varchar(2048) | Original URL                |
| `short_url`      | varchar(255)  | Full shortened URL          |
| `code`           | varchar(10)   | Unique short code (indexed) |
| `created_on_utc` | timestamp     | Creation timestamp          |
| `is_expiring`    | boolean       | Has expiration date?        |
| `expires_at`     | timestamp     | Expiration date (nullable)  |
| `attempt_count`  | integer       | Access count                |

**Indexes:**

- Primary key: `id`
- Unique index: `code`
- Index: `expires_at` (WHERE is_expiring = true)

### Table: short_url_tracks

Stores URL access records.

| Column             | Type         | Description                     |
| ------------------ | ------------ | ------------------------------- |
| `id`             | uuid         | Primary key                     |
| `shorten_url_id` | uuid         | Foreign key → shorten_urls(id) |
| `code`           | varchar(10)  | Short code (denormalized)       |
| `ip_address`     | varchar(20)  | Client IP address               |
| `user_agent`     | varchar(500) | Client user agent               |
| `accessed_at`    | timestamp    | Access timestamp                |
| `created_on_utc` | timestamp    | Record creation timestamp       |

**Indexes:**

- Primary key: `id`
- Foreign key: `shorten_url_id`
- Index: `code` (for fast lookup)
- Index: `accessed_at` (for date-based queries)

## 🐳 Docker Commands

### Build Docker Image

```bash
docker build -t url-shortener:latest .
```

### Run Docker Container

```bash
docker run -d -p 8080:8080 \
  -e PostgresOptions__ConnectionString="Server=host.docker.internal;Port=5432;Database=shorten_new;Username=postgres;Password=123456" \
  -e RedisOptions__Host=host.docker.internal \
  -e RedisOptions__Port=6379 \
  -e RedisOptions__Password=redis123 \
  --name url-shortener \
  url-shortener:latest
```

### View Logs

```bash
# Docker Compose
docker-compose logs -f app

# Docker Container
docker logs -f url-shortener

# Last 100 lines
docker logs --tail 100 url-shortener
```

### Access Container Shell

```bash
# Docker Compose
docker-compose exec app /bin/bash

# Docker Container
docker exec -it url-shortener /bin/bash
```

### Check Container Status

```bash
# List all containers
docker ps -a

# Docker Compose services
docker-compose ps

# Container resource usage
docker stats url-shortener
```

## 🛠️ Development

### Build the Project

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Create Migration

**For PostgreSQL:**

```bash
cd src/Infrastructure/UrlShortener.Infrastructure.EfCore.Postgres
dotnet ef migrations add <MigrationName> \
  --startup-project ../../Host/UrlShortener.Host
```

**For SQL Server:**

```bash
cd src/Infrastructure/UrlShortener.Infrastructure.EfCore.SqlServer
dotnet ef migrations add <MigrationName> \
  --startup-project ../../Host/UrlShortener.Host
```

### Apply Migrations

```bash
cd src/Host/UrlShortener.Host

# PostgreSQL
dotnet ef database update \
  --project ../../Infrastructure/UrlShortener.Infrastructure.EfCore.Postgres

# SQL Server
dotnet ef database update \
  --project ../../Infrastructure/UrlShortener.Infrastructure.EfCore.SqlServer
```

### Clean Solution

```bash
dotnet clean
```

### Restore NuGet Packages

```bash
dotnet restore
```

## 🚀 Performance Optimizations

The application uses the following optimization techniques:

- ✅ **ValueTask\<T\>** - Prevents allocation for synchronous paths
- ✅ **stackalloc** - Stack memory usage for temporary buffers
- ✅ **String interpolation** - Faster than `string.Format`
- ✅ **Query projections** - Avoids loading unnecessary data
- ✅ **HybridCache** - Two-level cache strategy
- ✅ **Distributed locking** - Ensures code uniqueness
- ✅ **Asynchronous event processing** - Non-blocking URL tracking
- ✅ **Connection pooling** - Database connection pool
- ✅ **Index strategy** - Optimized database queries
- ✅ **Minimal APIs** - Low-overhead API endpoints

### Benchmark Results (Example)

| Operation             | Average Time | Throughput |
| --------------------- | ------------ | ---------- |
| URL Shortening        | ~15ms        | ~66 req/s  |
| Redirect (Cache Hit)  | ~2ms         | ~500 req/s |
| Redirect (Cache Miss) | ~25ms        | ~40 req/s  |
| URL Lookup            | ~3ms         | ~333 req/s |

*Note: Results vary based on hardware.*

## 🔧 Supported Databases

The project uses a **provider-agnostic architecture** - include only the database provider you need:

### PostgreSQL (Current Default)

```csharp
builder.Services.AddShortenPostgres(builder.Configuration);
await app.Services.ApplyPostgresDatabaseMigrationsAsync();
```

- ✅ Production-ready
- ✅ High performance
- ✅ JSONB support
- ✅ Docker image available

### SQL Server

```csharp
builder.Services.AddShortenSqlServer(builder.Configuration);
await app.Services.ApplySqlServerDatabaseMigrationsAsync();
```

- ✅ Enterprise-compatible
- ✅ Azure SQL Database support
- ✅ Advanced security features

### MongoDB

```csharp
builder.Services.AddShortenMongoDb(builder.Configuration);
// No migrations needed (schema-less)
```

- ✅ NoSQL flexibility
- ✅ Horizontal scaling
- ✅ Document-based queries

**To switch databases:** Update `Program.cs` with the desired provider extension and configure connection string in `appsettings.json`. See [DATABASE_PROVIDER_GUIDE.md](DATABASE_PROVIDER_GUIDE.md) for details.

## 📨 Message Bus Options

The project supports multiple messaging providers - choose what you need:

### InMemory (Current Default)

```csharp
builder.Services.AddInMemoryMasstransit(builder.Configuration);
```

- ✅ Easy setup
- ✅ Ideal for development environment
- ❌ Limited to single instance

### RabbitMQ (Production)

```csharp
builder.Services.AddRabbitMqMasstransit(builder.Configuration);
```

- ✅ Distributed messaging
- ✅ High reliability
- ✅ Message persistence
- ✅ Scalable architecture

**To switch to RabbitMQ:** Update `Program.cs` with `AddRabbitMqMasstransit()` and configure RabbitMQ connection in `appsettings.json`.

## 🧪 Testing

The project includes comprehensive test coverage across all layers with **39 tests**.

### Test Projects

- **UrlShortener.Domain.Tests** (16 tests) - Domain entities validation and behavior
- **UrlShortener.Application.Tests** (8 tests) - Application services with full mocking
- **UrlShortener.Infrastructure.Tests** (15 tests) - Store and Redis integration tests

### Quick Start - Running Tests Without Docker

Most tests don't require Docker and can run immediately:

```bash
# Run all unit tests (33 tests - no Docker needed)
dotnet test tests/UrlShortener.Domain.Tests/
dotnet test tests/UrlShortener.Application.Tests/
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~ShortenUrlStoreTests"
```

**Test Results:**
- ✅ Domain Tests: 16 passed
- ✅ Application Tests: 8 passed
- ✅ Infrastructure Store Tests: 9 passed
- **Total: 33 tests passing without Docker**

### Running Full Test Suite (With Docker)

Some integration tests use TestContainers and require Docker:

```bash
# Ensure Docker is running
docker ps

# Run all tests including integration tests (39 tests)
dotnet test

# Run Docker-dependent tests only
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~DistributedLockTests"
```

### Test Coverage

The test suite covers:

- ✅ **Domain Entities** (16 tests) - Entity creation, validation, expiration, tracking
- ✅ **Application Services** (8 tests) - Business logic, code collision, caching
- ✅ **Infrastructure Store** (9 tests) - PostgreSQL store with EF Core InMemory
- ✅ **Redis/Distributed Locks** (6 tests) - Real Redis via TestContainers (requires Docker)

### Testing Tools

- **xUnit 3.1.4** - Test framework
- **Moq 4.20.72** - Mocking framework
- **FluentAssertions 8.8.0** - Fluent assertions
- **TestContainers 4.10.0** - Docker-based integration testing

For detailed testing documentation, see [TESTING.md](TESTING.md).

## 📝 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Feel free to submit a Pull Request.

### Contributing Steps

1. Fork the project
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Message Format

We use the Conventional Commits standard:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation change
- `refactor:` - Code refactoring
- `test:` - Adding/updating tests
- `chore:` - Build/config changes

## 🐛 Reporting Issues

If you find a bug or have a suggestion, please open an [issue](../../issues).

---

**Developer:** [GitHub Profile](https://github.com/oguzVstudio)
**Project Link:** [urlShortener](https://github.com/oguzVstudio/urlShortener)

⭐ Don't forget to star the project if you like it!
