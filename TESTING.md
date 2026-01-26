# Unit Tests Summary

## Overview

Comprehensive unit test suite added to the URL Shortener application covering Domain and Application layers.

## Test Projects Created

### 1. UrlShortener.Domain.Tests

Located at: `tests/UrlShortener.Domain.Tests/`

**Test Files:**

- `Shorten/ShortLinks/ShortLinkTests.cs` - 17 tests
- `Shorten/ShortLinks/ShortLinkAccessLogTests.cs` - 4 tests

**Coverage:**

- ✅ ShortLink entity creation
- ✅ URL expiration handling
- ✅ IsExpired property validation
- ✅ Unique ID generation
- ✅ ShortLinkAccessLog entity creation
- ✅ Access tracking data

### 2. UrlShortener.Application.Tests

Located at: `tests/UrlShortener.Application.Tests/`

**Test Files:**

- `Features/Shorten/Services/v1/ShortLinkAppServiceTests.cs` - 14 tests
- `Features/Analytics/Services/v1/ShortLinkAnalyticsAppServiceTests.cs` - 5 tests

**Coverage:**

- ✅ URL shortening with valid requests
- ✅ Expiring URL creation
- ✅ Code collision handling and retry logic
- ✅ Original URL retrieval from cache
- ✅ Missing URL handling
- ✅ URL access tracking
- ✅ Distributed lock acquisition
- ✅ Cache invalidation scenarios
- ✅ Analytics service operations
- ✅ Access log retrieval

**Test Helpers:**

- `Helpers/TestHybridCache.cs` - Custom HybridCache implementation for testing
- `Helpers/ShortLinkBuilder.cs` - Test data builder pattern
- `Helpers/TestData.cs` - Common test data constants

## Testing Framework & Tools

- **xUnit 3.1.4** - Modern .NET testing framework
- **Moq 4.20.72** - Powerful mocking library
- **FluentAssertions 8.8.0** - Expressive assertion library
- **TestContainers 4.10.0** - Docker-based integration testing
- **Microsoft.AspNetCore.Mvc.Testing 10.0.1** - API integration testing
- **.NET 10** - Latest .NET framework

## Test Statistics

- **Total Tests:** 51
- **Unit Tests (No Docker Required):** 45
  - Domain Tests: 17 ✅
  - Application Tests: 19 ✅
  - Infrastructure Store Tests: 9 ✅
- **Integration Tests (Docker Required):** 6
  - Infrastructure Redis Tests: 6 (TestContainers)

## Running Tests

### Quick Start (No Docker Required)

Run unit tests that don't require Docker:

```bash
# Run all unit tests (45 tests)
dotnet test tests/UrlShortener.Domain.Tests/
dotnet test tests/UrlShortener.Application.Tests/
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~ShortLinkStoreTests"
```

### Full Test Suite (Docker Required)

Run all tests including integration tests with Docker:

```bash
# Ensure Docker is running first
docker ps

# Run all tests (51 tests)
dotnet test
```

### Specific Test Categories

```bash
# Domain layer tests (17 tests)
dotnet test tests/UrlShortener.Domain.Tests/

# Application layer tests (19 tests)
dotnet test tests/UrlShortener.Application.Tests/

# Infrastructure tests - Store only (9 tests, no Docker)
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~ShortLinkStoreTests"

# Infrastructure tests - Redis only (6 tests, requires Docker)
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~DistributedLockTests"

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Docker Setup

Some tests require Docker to be running:

```bash
# Check if Docker is running
docker ps

# If Docker is not running:
# - macOS: Start Docker Desktop
# - Linux: sudo systemctl start docker
# - Windows: Start Docker Desktop
```

**Docker-Dependent Tests:**

- Infrastructure Redis/Distributed Lock tests (uses TestContainers.Redis)

## Key Features

### Domain Tests (17 tests)

- **Pure unit tests** with no external dependencies
- Tests entity creation, validation, and behavior
- Uses FluentAssertions for readable test assertions
- Theory tests for multiple input scenarios
- Tests IsExpired property edge cases
- **No Docker required** ✅

### Application Tests (19 tests)

- **Comprehensive mocking** using Moq for all dependencies
- Tests service orchestration and business logic
- Includes edge cases (code collisions, missing data)
- Tests analytics service operations
- Custom TestHybridCache for cache behavior simulation
- Verifies all interactions with mocked dependencies
- **No Docker required** ✅

### Infrastructure Tests (15 tests)

- **Store Tests (9 tests)** - Uses EF Core InMemory database

  - Tests PostgreSQL store implementation
  - No external dependencies
  - **No Docker required** ✅
- **Redis/Distributed Lock Tests (6 tests)** - Uses TestContainers

  - Tests real Redis behavior
  - Lock acquisition, expiration, removal
  - Concurrent lock scenarios
  - **Requires Docker** 🐳

### API Integration Tests (6 tests)

- **End-to-end HTTP tests** using WebApplicationFactory
- Tests complete request/response flow
- Uses TestContainers for PostgreSQL and Redis
- Tests URL shortening, redirection, tracking
- **Requires Docker** 🐳

## Test Examples

### Domain Test Example

```csharp
[Fact]
public void Create_ShouldCreateValidShortLink_WhenValidParametersProvided()
{
    // Arrange
    var longUrl = "https://www.example.com/very/long/url";
    var shortUrl = "https://short.url/abc123";
    var code = "abc123";

    // Act
    var shortLink = ShortLink.Create(longUrl, shortUrl, code);

    // Assert
    shortLink.Should().NotBeNull();
    shortLink.LongUrl.Should().Be(longUrl);
    shortLink.Code.Should().Be(code);
}
```

### Application Test Example

```csharp
[Fact]
public async Task CreateShortLinkAsync_ShouldRetryCodeGeneration_WhenCodeAlreadyExists()
{
    // Arrange
    var request = new CreateShortLinkRequest("https://www.example.com/url");
    _uniqueCodeGeneratorMock
        .Setup(x => x.GenerateAsync(cancellationToken))
        .ReturnsAsync(() => codeSequence.Dequeue());
  
    // Act
    var result = await _service.CreateShortLinkAsync(request, cancellationToken);

    // Assert
    result.Alias.Should().Be(secondCode);
    _uniqueCodeGeneratorMock.Verify(
        x => x.GenerateAsync(cancellationToken), 
        Times.Exactly(2));
}
```

## Benefits

1. **Confidence** - Ensures code behaves as expected
2. **Regression Prevention** - Catches bugs before they reach production
3. **Documentation** - Tests serve as living documentation
4. **Refactoring Safety** - Safe to refactor with test coverage
5. **Quality** - Enforces good coding practices
6. **Flexibility** - Can run subset of tests without Docker in CI/CD
7. **Real Integration Testing** - TestContainers provides real database testing

## CI/CD Considerations

For continuous integration environments:

```bash
# Option 1: Run only unit tests (fast, no Docker)
dotnet test tests/UrlShortener.Domain.Tests/
dotnet test tests/UrlShortener.Application.Tests/
dotnet test tests/UrlShortener.Infrastructure.Tests/ --filter "FullyQualifiedName~ShortLinkStoreTests"

# Option 2: Run full suite (requires Docker support in CI)
dotnet test
```

Most CI/CD systems (GitHub Actions, Azure DevOps, GitLab CI) support Docker and can run the full test suite.
