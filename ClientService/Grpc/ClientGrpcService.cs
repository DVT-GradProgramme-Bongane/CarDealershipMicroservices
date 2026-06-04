using ClientServices.Data;
using Grpc.Core;

namespace ClientCustomerService.Grpc;

public class ClientGrpcService : global::ClientService.ClientServiceBase
{
    private readonly ClientDbContext _context;

    public ClientGrpcService(ClientDbContext context)
    {
        _context = context;
    }

    public override async Task<ClientResponse> GetClient(GetClientRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var guid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format."));
        }

        var customer = await _context.Customers.FindAsync(guid);

        if (customer == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.Id} not found."));
        }

        return new ClientResponse
        {
            Id = customer.Id.ToString(),
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email
        };
    }
}
