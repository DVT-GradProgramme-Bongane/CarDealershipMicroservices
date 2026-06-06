using System.Net.Http.Json;
using FluentAssertions;
using Grpc.Core;

namespace CarDealership.StaffService.Tests.Integration.Grpc;

[Collection("Integration")]
public class StaffGrpcTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public StaffGrpcTests(IntegrationTestFixture fixture)
        => _fixture = fixture;

    public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetStaff_WithValidId_ReturnsCorrectStaffResponse()
    {
        var createPayload = new
        {
            firstName = "Thabo",
            lastName = "Nkosi",
            role = 0,
            email = "thabo.nkosi@dealer.test",
            phone = "+27 82 555 0100"
        };
        var created = await _fixture.Client.PostAsJsonAsync("/", createPayload);
        var employee = await created.Content.ReadFromJsonAsync<StaffEntitiy>();

        var grpcClient = new global::StaffService.StaffServiceClient(_fixture.GrpcChannel);
        var response = await grpcClient.GetStaffAsync(new GetStaffRequest { Id = employee!.Id.ToString() });

        response.Should().NotBeNull();
        response.Id.Should().Be(employee.Id.ToString());
        response.FirstName.Should().Be("Thabo");
        response.LastName.Should().Be("Nkosi");
        response.Role.Should().Be("salesperson");
    }

    [Fact]
    public async Task GetStaff_WithNonExistentId_ThrowsRpcNotFound()
    {
        var grpcClient = new global::StaffService.StaffServiceClient(_fixture.GrpcChannel);

        var act = async () => await grpcClient.GetStaffAsync(
            new GetStaffRequest { Id = Guid.NewGuid().ToString() });

        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
