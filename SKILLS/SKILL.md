# SKILL: Per-Service Test Scaffold & CI Pipeline

## Purpose
This skill tells the agent how to scaffold or **append to** an existing test suite
and generate a GitHub Actions CI workflow for a single .NET microservice in the
CarDealershipMicroservices monorepo.

The agent is always invoked for ONE service at a time. It never touches other
services. It never generates for the entire system.

---

## Critical rules before anything else

1. **Read before write** — every placeholder must be resolved from actual files
2. **Never write credentials** — no passwords, connection string passwords, or
   secret values in any generated file. Use environment variable references only
3. **Never create a git branch** — work against the current branch as-is
4. **Append, don't overwrite** — if a test file already exists for this service,
   add new tests to it. Never delete or regenerate an existing test file
5. **All .NET — no Node.js** — this skill only applies to .NET service projects
6. **Security check** — before writing any file, verify it contains no raw
   credentials. If a read step surfaces a password from `.env`, use only the
   key name (e.g. `${DB_PASSWORD}`) in output, never the value

---

## Step 0 — Orient from the README

Read `README.md` at the solution root first. Extract:
- What this service is responsible for in the dealership domain
- Which other services it depends on or is called by
- Any architectural notes relevant to testing (e.g. event-driven flows)

This context shapes what the tests are actually asserting — a staff service test
should assert staff domain behaviour, not generic CRUD.

---

## Critical path rule — test folder location

The test project lives at:
```
<ServiceName>/<ServiceName>.Tests/
```

NOT at `<SolutionRoot>/<ServiceName>.Tests/`. Every `dotnet new`, `dotnet add`,
file write, and `ProjectReference` must use the path inside the service folder.
Verify the target path before creating any file or running any command.

---

## Step 1 — Read the service manifest (do NOT re-read raw source files)

The orchestrating script has already extracted the key facts from the service
into a manifest object appended to this prompt. Read the manifest — it contains
everything needed to generate tests. Re-reading the raw `.csproj`, `Program.cs`,
or other source files wastes tokens and slows the run.

From the manifest, extract and hold in memory:
- `serviceName`, `serviceDir`, `testsDir`
- `targetFramework`, `dotnetVersion` → for the CI workflow `dotnet-version`
- `rootNamespace` → for all `namespace` declarations in generated files
- `dbContext` → for `DbContextOptions<>` and fixture overrides
- `serviceInterface`, `serviceImpl` → for unit test class setup
- `hasGrpc`, `grpcServices` → skip `Grpc/` folder entirely if `hasGrpc` is false
- `hasRabbitMq` → skip RabbitMQ container in fixture if false
- `migrationStrategy` → `MigrateAsync()` or `EnsureCreatedAsync()` in fixture
- `entities` → entity class names for unit tests
- `endpointGroups` → base URL paths for REST integration tests
- `infrastructure.postgresPort`, `infrastructure.rabbitmqPort` → for fixture config
- `existingTests.hasTests` → determines scaffold vs append mode

**Only read a source file if you need something the manifest does not contain**
(e.g. the exact constructor signature of an entity class for unit test setup).
If you do read a source file, extract only the specific fact needed and discard
the rest immediately.

---

## Step 2 — Determine run mode

### First-time scaffold
`<ServiceName>.Tests/` does not exist. Create the full structure from Step 3
onward, then generate the CI workflow.

### Append run
`<ServiceName>.Tests/` already exists. Read existing tests, identify what
endpoints or methods are not yet covered, and add only the missing tests.
Do not regenerate the fixture, do not regenerate the CI workflow unless the
workflow file is missing.

---

## Step 3 — Project structure

### First-time only — create the project

```bash
# Tests live INSIDE the service folder
cd <ServiceName>
dotnet new xunit -n <ServiceName>.Tests -o <ServiceName>.Tests
cd <ServiceName>.Tests

# Reference the service — one level up since we're inside the service folder
dotnet add reference ../<ServiceName>.csproj
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package Testcontainers.RabbitMq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Grpc.Net.Client
dotnet add package Respawn
```

Then add to the solution:
```bash
cd <SolutionRoot>
dotnet sln add <ServiceName>/<ServiceName>.Tests/<ServiceName>.Tests.csproj
```

Do not pin package versions — let NuGet resolve the best compatible version
for the target framework.

### Folder structure

```
<ServiceName>/                          ← service folder
└── <ServiceName>.Tests/                ← test project lives HERE, inside the service
    ├── <ServiceName>.Tests.csproj
├── appsettings.Testing.json        ← env var references only, no values
├── Unit/
│   ├── Services/
│   │   └── <ServiceName>ServiceTests.cs
│   └── Entities/
│       └── <EntityName>EntityTests.cs
└── Integration/
    ├── Fixtures/
    │   └── IntegrationTestFixture.cs
    ├── Rest/
    │   └── <ServiceName>EndpointsTests.cs
    └── Grpc/
        └── <ServiceName>GrpcTests.cs   ← omit entirely if no gRPC impl found
```

---

## Step 4 — `appsettings.Testing.json`

**No credential values. Environment variable references only.**

```json
{
  "ConnectionStrings": {
    "<DbContextName>": "Host=localhost;Port=5432;Database=<ServiceName>_test;Username=${DB_USER};Password=${DB_PASSWORD}"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": "5672"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

The actual credentials are injected at runtime by `IntegrationTestFixture`
via `WebApplicationFactory` overrides — the settings file is never the
source of truth for credentials in tests.

---

## Step 5 — `IntegrationTestFixture.cs`

Testcontainers manages real PostgreSQL and RabbitMQ containers. No
docker-compose dependency. Containers start before any test in the
collection and are torn down after.

The `Respawn` package resets database state between individual tests so
tests are fully isolated without restarting containers.

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Respawn;
using Grpc.Net.Client;

// Resolved from Step 1 reads:
// <DbContextName>   → actual DbContext class name
// <ServiceName>     → actual service name
// HasRabbitMQ       → true if publisher/consumer found in Program.cs

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture> { }

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    // Include only if service has RabbitMQ publisher or consumer:
    private readonly RabbitMqContainer _rabbitmq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private Respawner _respawner = null!;

    public HttpClient Client { get; private set; } = null!;
    public GrpcChannel GrpcChannel { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        // await _rabbitmq.StartAsync(); // include if HasRabbitMQ

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseEnvironment("Testing");
                host.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<<DbContextName>>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Register with Testcontainers connection string — no hardcoded credentials
                    services.AddDbContext<<DbContextName>>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));

                    // Override RabbitMQ if applicable:
                    // services.Configure<RabbitMQOptions>(o => {
                    //     o.Host = _rabbitmq.Hostname;
                    //     o.Port = _rabbitmq.GetMappedPublicPort(5672);
                    // });
                });
            });

        // Run migrations or ensure schema exists
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<<DbContextName>>();
        // Use MigrateAsync if Migrations/ folder is populated, else EnsureCreatedAsync:
        await db.Database.MigrateAsync();     // ← swap per Step 1 finding
        // await db.Database.EnsureCreatedAsync();

        Client = _factory.CreateClient();

        GrpcChannel = GrpcChannel.ForAddress(
            _factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpClient = _factory.CreateClient() });

        // Set up Respawn for between-test DB resets
        using var conn = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    // Call this from each integration test class's constructor or BeforeEach
    public async Task ResetDatabaseAsync()
    {
        using var conn = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        // await _rabbitmq.DisposeAsync(); // include if HasRabbitMQ
        await _factory.DisposeAsync();
    }
}
```

---

## Step 6 — Unit test patterns

Unit tests use in-memory database. No containers, no network, fast.

### Service tests — one test class per service interface method

```csharp
public class <ServiceName>ServiceTests
{
    private readonly <DbContextName> _context;
    private readonly <IServiceInterface> _service;

    public <ServiceName>ServiceTests()
    {
        var options = new DbContextOptionsBuilder<<DbContextName>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique per test
            .Options;
        _context = new <DbContextName>(options);
        _service = new <ServiceImplementation>(_context);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _service.GetAllAsync(CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenSeeded_ReturnsAllEntities()
    {
        // Arrange — seed directly via context, not via service
        _context.<DbSet>.Add(new <Entity>(<valid constructor args>));
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsEntity()
    {
        var entity = new <Entity>(<valid constructor args>);
        _context.<DbSet>.Add(entity);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(entity.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsAndReturnsEntity()
    {
        var request = new <CreateBody>(<valid test values>);
        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        _context.<DbSet>.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesEntity()
    {
        var entity = new <Entity>(<valid constructor args>);
        _context.<DbSet>.Add(entity);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(entity.Id, CancellationToken.None);

        _context.<DbSet>.Should().BeEmpty();
    }
}
```

### Entity tests

```csharp
public class <EntityName>EntityTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var entity = new <Entity>(<valid constructor args>);
        entity.FirstName.Should().Be(<expected>);
        // assert every property set by constructor
    }

    [Fact]
    public void Constructor_GeneratesNonEmptyId()
    {
        var entity = new <Entity>(<valid constructor args>);
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_SetsCreatedAt_ToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new <Entity>(<valid constructor args>);
        entity.CreatedAt.Should().BeOnOrAfter(before);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
```

---

## Step 7 — Integration test patterns

### REST endpoint tests

```csharp
[Collection("Integration")]
public class <ServiceName>EndpointsTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public <ServiceName>EndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    // Reset DB between each test for full isolation
    public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GET_Returns200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/<servicepath>");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<<Entity>>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task POST_WithValidBody_Returns201()
    {
        var payload = new { <valid fields matching CreateBody> };
        var response = await _client.PostAsJsonAsync("/api/<servicepath>", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_WithMissingRequiredField_Returns400()
    {
        var payload = new { /* intentionally missing required field */ };
        var response = await _client.PostAsJsonAsync("/api/<servicepath>", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/<servicepath>/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ById_WhenExists_Returns200()
    {
        // Seed via POST first
        var created = await _client.PostAsJsonAsync("/api/<servicepath>",
            new { <valid fields> });
        var location = created.Headers.Location!.ToString();

        var response = await _client.GetAsync(location);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### gRPC tests — omit this file entirely if no gRPC impl found in Step 1

```csharp
[Collection("Integration")]
public class <ServiceName>GrpcTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public <ServiceName>GrpcTests(IntegrationTestFixture fixture)
        => _fixture = fixture;

    public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get<Entity>_WithValidId_ReturnsResponse()
    {
        // Seed via HTTP client so test doesn't bypass service layer
        var created = await _fixture.Client.PostAsJsonAsync(
            "/api/<servicepath>", new { <valid fields> });
        var entity = await created.Content.ReadFromJsonAsync<<Entity>>();

        var grpcClient = new <ServiceName>Service.<ServiceName>ServiceClient(
            _fixture.GrpcChannel);
        var response = await grpcClient.Get<Entity>Async(
            new Get<Entity>Request { Id = entity!.Id.ToString() });

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id.ToString());
    }

    [Fact]
    public async Task Get<Entity>_WithInvalidId_ThrowsNotFound()
    {
        var grpcClient = new <ServiceName>Service.<ServiceName>ServiceClient(
            _fixture.GrpcChannel);

        var act = async () => await grpcClient.Get<Entity>Async(
            new Get<Entity>Request { Id = Guid.NewGuid().ToString() });

        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
```

---

## Step 8 — GitHub Actions CI workflow

One workflow file per service. Created at:
`.github/workflows/<service-name>-ci.yml`

Only regenerate if it does not already exist.

```yaml
name: <ServiceName> CI

on:
  push:
    paths:
      - '<ServiceName>/**'
      - '<ServiceName>.Tests/**'
      - 'Shared/**'
      - 'docker-compose.yml'
      - '.github/workflows/<service-name>-ci.yml'
  pull_request:
    paths:
      - '<ServiceName>/**'
      - '<ServiceName>.Tests/**'
      - 'Shared/**'
      - 'docker-compose.yml'
      - '.github/workflows/<service-name>-ci.yml'

# Prevent duplicate runs — cancel in-progress run on the same branch
# when a new push arrives
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    timeout-minutes: 20      # hard ceiling — agent or test loop cannot run forever

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '<VERSION>.x'    # resolved from TargetFramework in Step 1

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj') }}
          restore-keys: nuget-${{ runner.os }}-

      - name: Restore
        run: dotnet restore <ServiceName>/<ServiceName>.csproj

      - name: Build service
        run: dotnet build <ServiceName>/<ServiceName>.csproj --no-restore -c Release

      - name: Build tests
        run: dotnet build <ServiceName>.Tests/<ServiceName>.Tests.csproj --no-restore -c Release

      - name: Run unit tests
        run: |
          dotnet test <ServiceName>.Tests/<ServiceName>.Tests.csproj \
            --no-build \
            -c Release \
            --filter "FullyQualifiedName~Unit" \
            --logger "github-actions;verbosity=normal" \
            --results-directory ./test-results/unit

      - name: Run integration tests
        run: |
          dotnet test <ServiceName>.Tests/<ServiceName>.Tests.csproj \
            --no-build \
            -c Release \
            --filter "FullyQualifiedName~Integration" \
            --logger "github-actions;verbosity=normal" \
            --results-directory ./test-results/integration
        env:
          DOTNET_ENVIRONMENT: Testing
          # Testcontainers pulls images — no docker-compose, no .env needed here

      - name: Upload test results
        if: always()       # upload even on failure so results are visible
        uses: actions/upload-artifact@v4
        with:
          name: test-results-<service-name>
          path: ./test-results/

  docker-build:
    runs-on: ubuntu-latest
    needs: build-and-test   # only runs if tests pass
    timeout-minutes: 15

    steps:
      - uses: actions/checkout@v4

      - name: Build Docker image
        run: |
          docker build \
            -f <ServiceName>/Dockerfile \
            -t <service-name>:${{ github.sha }} \
            .
```

---

## Step 9 — Verify before finishing

The agent must run these commands and fix any errors before stopping:

```bash
# 1. Build compiles cleanly
dotnet build <ServiceName>.Tests/<ServiceName>.Tests.csproj

# 2. Unit tests run (fast — no containers needed)
dotnet test <ServiceName>.Tests/<ServiceName>.Tests.csproj \
  --filter "FullyQualifiedName~Unit"

# 3. Project is in the solution
dotnet sln list | grep <ServiceName>.Tests
```

Integration tests are NOT required to pass locally — they need Docker running
and will be validated by the CI pipeline. Unit tests MUST pass before the
agent is done.

If `dotnet build` fails, read the errors, fix the generated files, and rebuild.
Do not stop until unit tests pass. Maximum 3 fix iterations — if still failing
after 3, output a clear summary of what is broken and why, then stop.

---

## Step 10 — Append mode rules

When `<ServiceName>.Tests/` already exists:

1. Read all existing test files to build a map of what is covered
2. Read the current service endpoints and methods to find what is NOT covered
3. Add new test methods to the bottom of existing test classes
4. Never delete, rename, or reorder existing tests
5. New tests follow the same naming and assertion style as existing ones
6. Do not regenerate `IntegrationTestFixture.cs` — only add tests that use it
7. Do not regenerate the CI workflow — it already covers the service

The agent adds a comment block before each appended section:

```csharp
// ── Appended by scaffold agent ────────────────────────────────
// Covers: <EndpointName> added <date>
// ─────────────────────────────────────────────────────────────
```

---

## Model-agnostic notes

This skill is designed to work with any LLM available via CLI
(Claude Code, Ollama, llama.cpp, etc.). Keep this in mind:

- Each step is self-contained so a smaller context window can process one
  step at a time if needed
- No step requires holding the entire service in memory simultaneously —
  read, extract the specific facts needed, discard the raw source
- The facts extracted in Step 1 (a short list of names and flags) are the
  only things carried forward into generation steps
- If running on a local model with a small context window, the orchestrating
  script can run steps sequentially, piping only the extracted facts between them