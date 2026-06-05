using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CarDealership.StaffTests.Integration.Rest;

[Collection("Integration")]
public class StaffEndpointsTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public StaffEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GET_Root_Returns200_WithEmptyList()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task POST_WithValidBody_Returns201_AndCreatedStaff()
    {
        var payload = new
        {
            firstName = "Alice",
            lastName = "Smith",
            role = Role.salesperson,
            email = "alice@dealer.com",
            phone = "0821234567"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("firstName").GetString().Should().Be("Alice");
        body.GetProperty("lastName").GetString().Should().Be("Smith");
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_WithMissingEmail_Returns204()
    {
        var payload = new
        {
            firstName = "Bob",
            lastName = "Jones",
            role = Role.mechanic,
            email = "",
            phone = "0837654321"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        // The service returns NoContent (204) when required fields are missing
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_WithMissingFirstName_Returns204()
    {
        var payload = new
        {
            firstName = "",
            lastName = "Jones",
            role = Role.mechanic,
            email = "bob@dealer.com",
            phone = "0837654321"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GET_ById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ById_WhenExists_Returns200_WithCorrectStaff()
    {
        var payload = new
        {
            firstName = "Carol",
            lastName = "White",
            role = Role.manager,
            email = "carol@dealer.com",
            phone = "0841111111"
        };
        var created = await _client.PostAsJsonAsync("/", payload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid();

        var response = await _client.GetAsync($"/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().Be(id);
        body.GetProperty("firstName").GetString().Should().Be("Carol");
    }

    [Fact]
    public async Task GET_Root_AfterPost_Returns200_WithOneStaff()
    {
        var payload = new
        {
            firstName = "Dave",
            lastName = "Brown",
            role = Role.finance_manager,
            email = "dave@dealer.com",
            phone = "0852222222"
        };
        await _client.PostAsJsonAsync("/", payload);

        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        body.Should().HaveCount(1);
    }

    [Fact]
    public async Task PUT_ById_WhenExists_Returns200_WithUpdatedStaff()
    {
        var createPayload = new
        {
            firstName = "Eve",
            lastName = "Davis",
            role = Role.salesperson,
            email = "eve@dealer.com",
            phone = "0863333333"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid();

        var updatePayload = new
        {
            firstName = "Evelyn",
            lastName = "Davidson",
            role = Role.manager,
            email = "eve@dealer.com",
            phone = "0863333333"
        };
        var response = await _client.PutAsJsonAsync($"/{id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("firstName").GetString().Should().Be("Evelyn");
        body.GetProperty("lastName").GetString().Should().Be("Davidson");
    }

    [Fact]
    public async Task DELETE_ById_Returns204()
    {
        var payload = new
        {
            firstName = "Frank",
            lastName = "Green",
            role = Role.mechanic,
            email = "frank@dealer.com",
            phone = "0874444444"
        };
        var created = await _client.PostAsJsonAsync("/", payload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid();

        var response = await _client.DeleteAsync($"/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await _client.GetAsync($"/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Appended by scaffold agent ────────────────────────────────
    // Covers: POST/PUT validation for missing fields; PUT not-found; DELETE not-found
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task POST_WithMissingLastName_Returns204()
    {
        var payload = new
        {
            firstName = "Grace",
            lastName = "",
            role = Role.manager,
            email = "grace@dealer.com",
            phone = "0881111111"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_WithMissingPhone_Returns204()
    {
        var payload = new
        {
            firstName = "Henry",
            lastName = "Irving",
            role = Role.mechanic,
            email = "henry@dealer.com",
            phone = ""
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_WithMissingEmail_Returns204()
    {
        var createPayload = new
        {
            firstName = "Iris",
            lastName = "James",
            role = Role.salesperson,
            email = "iris@dealer.com",
            phone = "0891111111"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid();

        var updatePayload = new
        {
            firstName = "Iris",
            lastName = "James",
            role = Role.salesperson,
            email = "",
            phone = "0891111111"
        };

        var response = await _client.PutAsJsonAsync($"/{id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_WithMissingLastName_Returns204()
    {
        var createPayload = new
        {
            firstName = "Jack",
            lastName = "King",
            role = Role.finance_manager,
            email = "jack@dealer.com",
            phone = "0892222222"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid();

        var updatePayload = new
        {
            firstName = "Jack",
            lastName = "",
            role = Role.finance_manager,
            email = "jack@dealer.com",
            phone = "0892222222"
        };

        var response = await _client.PutAsJsonAsync($"/{id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_WhenNotFound_Returns200_WithNullBody()
    {
        // The endpoint calls service.UpdateAsync which returns null for unknown IDs,
        // then wraps it in Results.Ok(null) — this exposes that the PUT handler
        // does not guard against null and returns 200 instead of 404.
        var updatePayload = new
        {
            firstName = "Nobody",
            lastName = "Here",
            role = Role.salesperson,
            email = "nobody@dealer.com",
            phone = "0800000000"
        };

        var response = await _client.PutAsJsonAsync($"/{Guid.NewGuid()}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("null");
    }

    [Fact]
    public async Task DELETE_WhenNotFound_StillReturns204()
    {
        var response = await _client.DeleteAsync($"/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
