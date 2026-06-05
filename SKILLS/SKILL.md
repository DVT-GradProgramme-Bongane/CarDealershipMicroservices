# SKILL: Microservice Test & CI Setup

## Purpose
This skill tells the agent how to scaffold a complete test suite and GitHub Actions
CI pipeline for a .NET microservice in the CarDealershipMicroservices monorepo.
Apply this skill once per service. Running it on `StaffService` produces
`StaffService.Tests/`. Running it on `ClientService` produces `ClientService.Tests/`.

---

## Repo layout assumptions
Before doing anything, the agent MUST read the actual folder structure:

```
<SolutionRoot>/
├── docker-compose.yml          ← shared infra, used by integration tests
├── .env                        ← connection strings and secrets
├── Shared/
│   └── Protos/                 ← .proto files shared across services
├── <ServiceName>/              ← the target service being tested
│   ├── <ServiceName>.csproj
│   ├── Dockerfile
│   ├── Program.cs
│   ├── Migrations/
│   └── ...
└── <ServiceName>.Tests/        ← agent creates this
```

The agent must inspect the target service's `.csproj`, `Program.cs`, and folder
structure before generating any file. It must not assume what packages,
namespaces, or patterns are used — it reads them first.

---

## Step 0 — Read before writing

Run these reads in order before touching any file:

1. Read `<ServiceName>/<ServiceName>.csproj` — get TargetFramework, package versions, namespace
2. Read `<ServiceName>/Program.cs` — get registered services, DbContext name, gRPC services, endpoint registrations
3. List `<ServiceName>/` — identify: entity classes, service interfaces, endpoint files, gRPC impl files
4. Read `docker-compose.yml` — get postgres and rabbitmq service names, ports, credentials
5. Read `.env` — get connection string patterns

Only after all five reads does the agent begin generating files.

---

## Step 1 — Create the test project

```bash
cd <SolutionRoot>
dotnet new xunit -n <ServiceName>.Tests -o <ServiceName>.Tests
```

Then add references:

```bash
cd <ServiceName>.Tests

# Reference the service under test
dotnet add reference ../<ServiceName>/<ServiceName>.csproj

# Core test packages
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing

# Integration test infrastructure
dotnet add package Testcontainers.PostgreSql
dotnet add package Testcontainers.RabbitMq

# EF Core in-memory for unit tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# gRPC testing
dotnet add package Grpc.Net.Client
```

Package versions must match the `TargetFramework` found in Step 0.
For `net10.0` use the latest stable versions. Do not hardcode versions —
use `dotnet add package <name>` without a version flag so NuGet resolves
the best compatible version.

---

## Step 2 — Folder structure to create

```
<ServiceName>.Tests/
├── <ServiceName>.Tests.csproj
├── Unit/
│   ├── Services/
│   │   └── <ServiceName>ServiceTests.cs      ← tests for IServiceInterface impl
│   └── Entities/
│       └── <EntityName>EntityTests.cs        ← entity construction, validation
├── Integration/
│   ├── Fixtures/
│   │   └── IntegrationTestFixture.cs         ← Testcontainers setup, WebApplicationFactory
│   ├── Rest/
│   │   └── <ServiceName>EndpointsTests.cs    ← HTTP endpoint tests via HttpClient
│   └── Grpc/
│       └── <ServiceName>GrpcTests.cs         ← gRPC client tests via GrpcChannel
└── appsettings.Testing.json
```

---

## Step 3 — `appsettings.Testing.json`

The agent generates this file using the actual postgres/rabbitmq credentials
read from `docker-compose.yml` and `.env` in Step 0:

```json
{
  "ConnectionStrings": {
    "<DbContextName>": "Host=localhost;Port=<PORT>;Database=<ServiceName>_test;Username=<USER>;Password=<PASS>"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": "<AMQP_PORT>"
  }
}
```

Port, username, and password are filled in from what was read — never hardcoded
as placeholders.

---

## Step 4 — `IntegrationTestFixture.cs`

This is the most important file. It:
- Spins up a real PostgreSQL container via Testcontainers (NOT docker-compose)
- Spins up a real RabbitMQ container via Testcontainers
- Creates a `WebApplicationFactory<Program>` that overrides the connection string
- Runs `MigrateAsync()` against the test database before any test runs
- Tears down containers after the test class completes

Template pattern:

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer _rabbitmq;
    public HttpClient Client { get; private set; } = null!;
    public GrpcChannel GrpcChannel { get; private set; } = null!;

    public IntegrationTestFixture()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("<ServiceName>_test")
            .WithUsername("<USER_FROM_COMPOSE>")
            .WithPassword("<PASS_FROM_COMPOSE>")
            .Build();

        _rabbitmq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitmq.StartAsync();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseSetting(
                    "ConnectionStrings::<DbContextName>",
                    _postgres.GetConnectionString());
                host.UseSetting("RabbitMQ:Host", _rabbitmq.Hostname);
                host.UseSetting("RabbitMQ:Port", _rabbitmq.GetMappedPublicPort(5672).ToString());
            });

        Client = factory.CreateClient();
        GrpcChannel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = factory.CreateClient()
        });
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _rabbitmq.DisposeAsync();
    }
}
```

The agent fills in the real `<DbContextName>`, credentials, and service name
from what it read in Step 0.

---

## Step 5 — Unit test patterns

### Service tests
- Use Moq to mock the `DbContext` OR use `UseInMemoryDatabase`
- Test every public method on the service implementation
- Cover: happy path, not found (null return), validation failures
- Do NOT test EF Core internals — test your service's behaviour

```csharp
public class <ServiceName>ServiceTests
{
    private readonly <DbContextName> _context;
    private readonly <IServiceInterface> _service;

    public <ServiceName>ServiceTests()
    {
        var options = new DbContextOptionsBuilder<<DbContextName>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new <DbContextName>(options);
        _service = new <ServiceImplementation>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsEntity() { ... }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull() { ... }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsEntity() { ... }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedEntity() { ... }
}
```

### Entity tests
- Test constructor: correct property assignment, `Id` is not empty Guid, `CreatedAt` is set
- Test any domain rules on the entity

```csharp
[Fact]
public void Constructor_SetsAllProperties_Correctly() { ... }

[Fact]
public void Constructor_GeneratesNonEmptyId() { ... }
```

---

## Step 6 — Integration test patterns

### REST endpoint tests
Use `IntegrationTestFixture.Client` (real `HttpClient` against real DB):

```csharp
[Collection("Integration")]
public class <ServiceName>EndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public <ServiceName>EndpointsTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GET_api_<servicename>_ReturnsOk() { ... }

    [Fact]
    public async Task POST_api_<servicename>_WithValidBody_ReturnsCreated() { ... }

    [Fact]
    public async Task GET_api_<servicename>_id_WhenNotFound_Returns404() { ... }
}
```

### gRPC tests
Use `IntegrationTestFixture.GrpcChannel`:

```csharp
[Fact]
public async Task GetStaff_WithValidId_ReturnsResponse()
{
    var client = new <ServiceName>Service.<ServiceName>ServiceClient(_fixture.GrpcChannel);
    var response = await client.Get<Entity>Async(new Get<Entity>Request { Id = seededId });
    response.Should().NotBeNull();
}

[Fact]
public async Task GetStaff_WithInvalidId_ThrowsNotFound()
{
    var client = new <ServiceName>Service.<ServiceName>ServiceClient(_fixture.GrpcChannel);
    var act = async () => await client.Get<Entity>Async(new Get<Entity>Request { Id = Guid.NewGuid().ToString() });
    await act.Should().ThrowAsync<RpcException>()
        .Where(e => e.StatusCode == StatusCode.NotFound);
}
```

---

## Step 7 — Add test project to solution

```bash
cd <SolutionRoot>
dotnet sln add <ServiceName>.Tests/<ServiceName>.Tests.csproj
```

---

## Step 8 — GitHub Actions workflow

The agent creates `.github/workflows/<service-name>-ci.yml`.

One workflow file per service. It triggers on push/PR to paths that affect
that service only — avoiding rebuilding everything on every commit.

```yaml
name: <ServiceName> CI

on:
  push:
    paths:
      - '<ServiceName>/**'
      - '<ServiceName>.Tests/**'
      - 'Shared/**'
      - 'docker-compose.yml'
  pull_request:
    paths:
      - '<ServiceName>/**'
      - '<ServiceName>.Tests/**'
      - 'Shared/**'
      - 'docker-compose.yml'

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '<VERSION_FROM_CSPROJ>.x'   # e.g. 10.0.x

      - name: Restore
        run: dotnet restore <ServiceName>/<ServiceName>.csproj

      - name: Build
        run: dotnet build <ServiceName>/<ServiceName>.csproj --no-restore -c Release

      - name: Run unit tests
        run: dotnet test <ServiceName>.Tests/<ServiceName>.Tests.csproj
          --filter "FullyQualifiedName~Unit"
          --no-build
          -c Release
          --logger "github-actions"

      - name: Run integration tests
        run: dotnet test <ServiceName>.Tests/<ServiceName>.Tests.csproj
          --filter "FullyQualifiedName~Integration"
          --no-build
          -c Release
          --logger "github-actions"
        env:
          DOTNET_ENVIRONMENT: Testing

  docker-build:
    runs-on: ubuntu-latest
    needs: build-and-test

    steps:
      - uses: actions/checkout@v4

      - name: Build Docker image
        run: |
          docker build \
            -f <ServiceName>/Dockerfile \
            -t <service-name>:${{ github.sha }} \
            .
```

Key rules:
- `dotnet-version` is read from the `TargetFramework` found in Step 0
- Integration tests use Testcontainers — no `docker-compose up` needed in CI
- Docker build uses solution root as context (matches the Dockerfile COPY paths)
- Unit and integration tests run as separate steps so failures are easy to identify
- The workflow only triggers when relevant paths change

---

## Rules the agent must follow

1. Read before writing — never assume a class name, namespace, or package version
2. All placeholder values (`<ServiceName>`, `<DbContextName>`, etc.) must be
   resolved from the actual files before any output is written
3. Never generate a test that imports a namespace that doesn't exist in the service
4. If a service has no gRPC implementation, skip the `Grpc/` test folder entirely
5. If a service has no RabbitMQ publisher/consumer, skip RabbitMQ container setup
   in the fixture and remove the `Testcontainers.RabbitMq` package reference
6. Every generated test must compile — no pseudocode, no `// TODO` stubs left
   in method bodies. Incomplete tests use `Assert.True(true)` as a placeholder
   with a `// SCAFFOLD` comment so they are findable
7. After all files are written, run `dotnet build <ServiceName>.Tests` and fix
   any compiler errors before finishing
