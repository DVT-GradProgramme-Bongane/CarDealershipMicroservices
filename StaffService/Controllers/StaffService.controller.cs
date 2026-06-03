

public class StaffServiceEndpoint
{
    public void MapStaffEndpoints(IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api/staff");

        apiGroup.MapGet("/", (ListStaffResponse clients) =>
        {
            
        });

        apiGroup.MapGet("/{id}", (Guid id, Staff staff) =>
        {
            
        });

         apiGroup.MapPost("/", (CreateStaffBody request) =>
        {
            
        });

        apiGroup.MapPut("/{id}", (Guid id, Staff staff) =>
        {
            
        });
   
        apiGroup.MapDelete("/{id}", (Guid id, Staff staff) =>
        {
            
        });
    }   
}