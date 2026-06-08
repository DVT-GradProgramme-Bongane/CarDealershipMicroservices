# PROMPT: Scaffold or append tests for a single .NET microservice

## How this prompt is used

This file is filled in and passed to the agent by `scaffold-tests.sh`.
Do not run it manually with `%%` placeholders still present.

Resolved paths for this run:
- Service name:    %%SERVICE_NAME%%
- Service folder:  %%SERVICE_DIR%%
- Tests folder:    %%TESTS_DIR%%   ← tests live INSIDE the service folder
- Solution root:   %%SOLUTION_ROOT%%

---

## Context

You are working inside the CarDealershipMicroservices monorepo — a distributed
.NET microservice system for a car dealership. Services communicate internally
via gRPC and publish domain events via RabbitMQ. Each service owns its own
PostgreSQL schema via EF Core. The API gateway routes external HTTP traffic
to the appropriate service.

Read `%%SOLUTION_ROOT%%/README.md` first. It describes the domain, service
responsibilities, and overall architecture. Use it to understand what behaviour
the tests should assert — not just that endpoints return 200, but that the
domain logic is correct for the dealership context.

---

## Your task

Set up or extend the test suite for: **%%SERVICE_NAME%%**

Read the SKILL.md at `%%SOLUTION_ROOT%%/skills/SKILL.md` and follow every
step in order. Do not skip Step 0 (README) or Step 1 (service reads).

### Critical path — test folder location

The test project must be created at:
```
%%TESTS_DIR%%
```

This is INSIDE the service folder, not at the solution root.
The `ProjectReference` in the test `.csproj` points to:
```
%%SERVICE_DIR%%/%%SERVICE_NAME%%.csproj
```

Verify these paths exist or will be created at the correct location before
writing any file.

---

## Security constraint — non-negotiable

You must not write any credential value into any file. This includes:
- Database passwords
- RabbitMQ passwords  
- Any value read from `.env`
- Any hardcoded connection string containing a real password

If you encounter a password during reads, discard the value immediately.
Use only the environment variable key name in generated files.
Testcontainers generates its own ephemeral credentials at runtime.

---

## Scope constraints

- This service only: %%SERVICE_NAME%%
- Do not modify any other service's files
- Do not modify `docker-compose.yml`
- Do not create a git branch
- Do not push to any registry
- Do not commit anything

---

## Run mode detection

Check whether `%%TESTS_DIR%%` already has test files (files ending in `Tests.cs`,
excluding any `UnitTest1.cs` default):

### If NO real test files exist — first-time scaffold
Create the full test project at `%%TESTS_DIR%%`, all test files, the fixture,
and the CI workflow as described in SKILL.md.

### If real test files already exist — append run
A developer has added new endpoints or methods since the last scaffold run.
1. Read existing tests to understand what is already covered
2. Read the current service to find what has no test yet
3. Append tests for only the new items
4. Leave all existing tests exactly as they are
5. Do not regenerate the fixture or CI workflow

---

## What the tests must do

Tests exist to expose errors in the service — not to pass unconditionally.
A test that correctly identifies a bug in the service is a success.

- Unit tests must assert actual return values and side effects, not just
  that methods don't throw
- Integration tests must assert HTTP status codes AND response body shape
- gRPC tests must assert response field values, not just that a response arrived
- Edge cases must be tested: not found, missing required fields, empty collections

---

## Reporting progress

After completing each major step, print a status line so the developer can
follow progress in the terminal:

```
[Step 0] README read — service is the staff management domain
[Step 1] Service read — DbContext: StaffDBContext, gRPC: yes, RabbitMQ: no
[Step 2] Mode: first-time scaffold
[Step 3] Created test project at /path/to/StaffService/StaffService.Tests
[Step 4] Written appsettings.Testing.json
[Step 5] Written IntegrationTestFixture.cs
[Step 6] Written Unit/Services/StaffServiceTests.cs (4 tests)
[Step 6] Written Unit/Entities/StaffEntityTests.cs (3 tests)
[Step 7] Written Integration/Rest/StaffEndpointsTests.cs (5 tests)
[Step 7] Written Integration/Grpc/StaffGrpcTests.cs (2 tests)
[Step 8] Written .github/workflows/staff-service-ci.yml
[Step 9] Build: PASSED
[Step 9] Unit tests: PASSED
[Done] Scaffold complete
```

Print these lines as each step finishes, not all at the end.

---

## Definition of done

You are finished when ALL of the following are true:

- [ ] README read and domain context understood
- [ ] All service files read (csproj, Program.cs, folder listing)
- [ ] No credential values in any generated file
- [ ] Test project exists at `%%TESTS_DIR%%`
- [ ] `dotnet build %%TESTS_DIR%%/%%SERVICE_NAME%%.Tests.csproj` passes with 0 errors
- [ ] `dotnet test %%TESTS_DIR%%/%%SERVICE_NAME%%.Tests.csproj --filter "FullyQualifiedName~Unit"` runs
- [ ] `.github/workflows/%%SERVICE_NAME_LOWER%%-ci.yml` exists (first-time only)
- [ ] Test project appears in `dotnet sln list` (first-time only)

If `dotnet build` fails, fix and retry. Maximum 3 fix iterations.
After 3 failed attempts, print exactly what is broken and what the developer
must fix manually — then stop cleanly.

Do not ask for confirmation between steps. Work through the full skill autonomously.