using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Grpc.Core;

// Namespace avoids shadowing the proto-generated StaffService class in the global namespace
namespace CarDealership.StaffTests.Integration.Grpc;

[Collection("Integration")]
public class StaffGrpcTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public StaffGrpcTests(IntegrationTestFixture fixture)
        => _fixture = fixture;

    public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetStaff_WithValidId_ReturnsCorrectStaff()
    {
        var payload = new
        {
            firstName = "Alice",
            lastName = "Smith",
            role = Role.salesperson,
            email = "alice@dealer.com",
            phone = "0821234567"
        };
        var created = await _fixture.Client.PostAsJsonAsync("/", payload);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetGuid().ToString();

        var grpcClient = new global::StaffService.StaffServiceClient(_fixture.GrpcChannel);
        var response = await grpcClient.GetStaffAsync(new GetStaffRequest { Id = id });

        response.Should().NotBeNull();
        response.Id.Should().Be(id);
        response.FirstName.Should().Be("Alice");
        response.LastName.Should().Be("Smith");
        response.Role.Should().Be(Role.salesperson.ToString());
    }

    [Fact]
    public async Task GetStaff_WithInvalidId_ThrowsRpcNotFound()
    {
        var grpcClient = new global::StaffService.StaffServiceClient(_fixture.GrpcChannel);

        var act = async () => await grpcClient.GetStaffAsync(
            new GetStaffRequest { Id = Guid.NewGuid().ToString() });

        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
