using Grpc.Core;
using Microsoft.EntityFrameworkCore;


public class GrpcStaffService : StaffService.StaffServiceBase
{
    private readonly StaffDBContext _db;

    public GrpcStaffService(StaffDBContext db)
    {
        _db = db;
    }

    public override async Task<StaffResponse> GetStaff(
        GetStaffRequest request, ServerCallContext context)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id.ToString() == request.Id);

        if (staff is null) throw new RpcException(new Status(StatusCode.NotFound, "Staff not found"));

        // Map EF Core entity → protobuf response
        return new StaffResponse
        {
            Id        = staff.Id.ToString(),
            FirstName = staff.FirstName,
            LastName  = staff.LastName,
            Role      = staff.StaffRole.ToString()
        };
    }
}