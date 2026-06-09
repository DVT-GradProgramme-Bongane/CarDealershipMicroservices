using Grpc.Core;
using Inventory.Api.Data;
using Inventory.Api.Models;

namespace Inventory.Api.Services;

// Base class is generated from the proto — lives in the global InventoryService class
public class InventoryGrpcService : InventoryService.InventoryServiceBase
{
    private readonly AppDbContext _db;
    private readonly RabbitMqPublisher _rabbit;

    public InventoryGrpcService(AppDbContext db, RabbitMqPublisher rabbit)
    {
        _db = db;
        _rabbit = rabbit;
    }

    public override async Task<CarResponse> GetCar(
        GetCarRequest request, ServerCallContext context)
    {
        var car = await _db.Cars.FindAsync(Guid.Parse(request.Id));
        if (car is null)
            throw new RpcException(
                new Status(StatusCode.NotFound, "Car not found"));

        return new CarResponse {
            Id = car.Id.ToString(), Vin = car.Vin,
            Make = car.Make, Model = car.Model, Status = car.Status.ToString()
        };
    }

    public override async Task<StatusResponse> UpdateCarStatus(
        UpdateStatusRequest request, ServerCallContext context)
    {
        var car = await _db.Cars.FindAsync(Guid.Parse(request.Id));
        if (car is null)
            return new StatusResponse { Success = false };
        // gRPC sends status as a raw string — parse it into the enum
        if (!Enum.TryParse<CarStatus>(request.Status, ignoreCase: true, out var parsed))
            return new StatusResponse { Success = false };
        car.Status = parsed;
        await _db.SaveChangesAsync();

        await _rabbit.PublishAsync("car.status.updated", new {
            car_id = car.Id, status = car.Status.ToString()
        });

        return new StatusResponse { Success = true };
    }
}