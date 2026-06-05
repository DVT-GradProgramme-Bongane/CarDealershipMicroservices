# PROMPT: Scaffold tests and CI for a microservice

## How to use this prompt

Replace `<ServiceName>` with the actual service you want to set up.
Example: `StaffService`, `ClientService`, `InventoryService`.

Then run this prompt with Claude Code from the solution root.

---

## Prompt

You are setting up a complete test suite and GitHub Actions CI pipeline for
`<ServiceName>` in this .NET microservice monorepo.

Before writing a single file, read the SKILL.md at:
`<SolutionRoot>/skills/SKILL.md`

Follow every step in that skill exactly. Do not skip Step 0.

---

### Your task

1. Read the service — understand its structure, packages, namespaces, DbContext,
   registered services, gRPC implementation, and REST endpoints before generating
   anything.

2. Scaffold the test project `<ServiceName>.Tests/` at the solution root with:
   - Unit tests for every public method on the service implementation
   - Unit tests for entity construction and validation
   - Integration tests for every REST endpoint (GET all, GET by id, POST, DELETE)
   - Integration tests for every gRPC method the service exposes
   - An `IntegrationTestFixture` using Testcontainers for real PostgreSQL and
     RabbitMQ (only include RabbitMQ if the service has a publisher or consumer)

3. Create `.github/workflows/<service-name>-ci.yml` that:
   - Triggers only when files under `<ServiceName>/`, `<ServiceName>.Tests/`,
     `Shared/`, or `docker-compose.yml` change
   - Restores, builds, runs unit tests, runs integration tests as separate steps
   - Builds the Docker image as a final job that only runs if tests pass
   - Uses the .NET version from the service's `.csproj`

4. Add the test project to the solution file.

5. Run `dotnet build <ServiceName>.Tests/<ServiceName>.Tests.csproj` and fix
   any compiler errors before you finish.

---

### Constraints

- Do not hardcode connection strings, ports, or credentials — read them from
  `docker-compose.yml` and `.env`
- Do not push to any Docker registry
- Do not use `docker-compose up` in CI — Testcontainers handles infrastructure
- Every test method must have a real body — no empty methods, no pseudocode
- Use FluentAssertions for all assertions (`result.Should().Be(...)`)
- Use Moq only where Testcontainers would be overkill (pure unit tests)
- All generated namespaces must match what actually exists in the service
- If the service has no gRPC, skip gRPC tests entirely
- Match the xUnit collection pattern: unit tests under `Unit/`, integration
  tests under `Integration/`

---

### Definition of done

You are finished when:
- [ ] `<ServiceName>.Tests/` exists with the full folder structure from the skill
- [ ] `dotnet build <ServiceName>.Tests` passes with 0 errors
- [ ] `dotnet test <ServiceName>.Tests --filter "FullyQualifiedName~Unit"` passes
- [ ] `.github/workflows/<service-name>-ci.yml` exists and is valid YAML
- [ ] The test project is registered in the `.sln` file

Do not stop until all five boxes are checked.
