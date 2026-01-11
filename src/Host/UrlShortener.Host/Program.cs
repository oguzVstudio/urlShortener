using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.Host.Extensions;
using UrlShortener.Host.Features.Shorten.UrlRedirection;
using UrlShortener.Infrastructure.Extensions;
using UrlShortener.Infrastructure.EfCore.Postgres.Extensions;
using UrlShortener.Infrastructure.Masstransit.InMemory.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointModules(Assembly.GetExecutingAssembly());

// Register core infrastructure (domain, application, cache, distributed lock)
builder.Services.AddInfrastructure(builder.Configuration);

// Register messaging provider (InMemory or RabbitMQ)
builder.Services.AddInMemoryMasstransit(builder.Configuration);
// Alternative: builder.Services.AddRabbitMqMasstransit(builder.Configuration);

// Register database provider (choose one)
builder.Services.AddShortenPostgres(builder.Configuration);        // PostgreSQL
//builder.Services.AddShortenSqlServer(builder.Configuration);    // SQL Server
// builder.Services.AddShortenMongoDb(builder.Configuration);     // MongoDB

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 5;// 5 requests
        opt.Window = TimeSpan.FromSeconds(10); // 10 seconds
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // No queue
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Apply database migrations (for EF Core providers: Postgres/SqlServer)
await app.Services.ApplyPostgresDatabaseMigrationsAsync();
//await app.Services.ApplySqlServerDatabaseMigrationsAsync(); // For SQL Server

app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapRedirectOriginalUrlEndpoint();

app.MapEndpointModules();

app.Run();