using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CarDealership.StaffService.Tests.Integration.Rest;

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
    public async Task GET_AllEmployees_WhenEmpty_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<StaffEntitiy>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task POST_WithValidBody_Returns201AndCreatedEmployee()
    {
        var payload = new
        {
            firstName = "Zanele",
            lastName = "Maseko",
            role = 0, // salesperson
            email = "zanele.maseko@dealer.test",
            phone = "+27 11 555 0199"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<StaffEntitiy>();
        body.Should().NotBeNull();
        body!.FirstName.Should().Be("Zanele");
        body.Email.Should().Be("zanele.maseko@dealer.test");
    }

    [Fact]
    public async Task POST_WithMissingEmail_Returns204()
    {
        var payload = new
        {
            firstName = "Zanele",
            lastName = "Maseko",
            role = 0,
            email = "",
            phone = "+27 11 555 0199"
        };

        var response = await _client.PostAsJsonAsync("/", payload);

        // Service returns NoContent (204) for missing required fields — this is the current behaviour
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GET_ById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ById_WhenExists_Returns200WithEmployee()
    {
        var createPayload = new
        {
            firstName = "Thabo",
            lastName = "Nkosi",
            role = 0,
            email = "thabo.nkosi@dealer.test",
            phone = "+27 82 555 0100"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var employee = await created.Content.ReadFromJsonAsync<StaffEntitiy>();

        var response = await _client.GetAsync($"/{employee!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StaffEntitiy>();
        body!.Id.Should().Be(employee.Id);
        body.FirstName.Should().Be("Thabo");
    }

    [Fact]
    public async Task PUT_ById_WhenExists_Returns200WithUpdatedEmployee()
    {
        var createPayload = new
        {
            firstName = "Pieter",
            lastName = "vanWyk",
            role = 0,
            email = "pieter@dealer.test",
            phone = "+27 11 555 0203"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var employee = await created.Content.ReadFromJsonAsync<StaffEntitiy>();

        var updatePayload = new
        {
            firstName = "Pieter",
            lastName = "van Wyk",
            role = 1, // finance_manager
            email = "pieter@dealer.test",
            phone = "+27 11 555 0203"
        };
        var response = await _client.PutAsJsonAsync($"/{employee!.Id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StaffEntitiy>();
        body!.LastName.Should().Be("van Wyk");
        body.StaffRole.Should().Be(Role.finance_manager);
    }

    [Fact]
    public async Task DELETE_ById_WhenExists_Returns204()
    {
        var createPayload = new
        {
            firstName = "Naledi",
            lastName = "Dlamini",
            role = 2, // mechanic
            email = "naledi@dealer.test",
            phone = "+27 11 555 0202"
        };
        var created = await _client.PostAsJsonAsync("/", createPayload);
        var employee = await created.Content.ReadFromJsonAsync<StaffEntitiy>();

        var response = await _client.DeleteAsync($"/{employee!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
