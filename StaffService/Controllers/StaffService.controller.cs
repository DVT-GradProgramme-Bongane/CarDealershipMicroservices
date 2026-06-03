
public static class StaffServiceEndpoint
{
    public static void MapStaffEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api/staff");

        apiGroup.MapGet("/", async (IStaffService service, CancellationToken token) =>
        {
            var staff = await service.GetAllAsync(token);
            return Results.Ok(staff);
        });

        apiGroup.MapGet("/{id}", async (Guid id, IStaffService service, CancellationToken token) =>
        {
            var staff = await service.GetByIdAsync(id, token);
            return staff is null
                ? Results.NotFound(new { Message = $"Staff member {id} not found" })
                : Results.Ok(staff);
        });

        apiGroup.MapPost("/", async (IStaffService service, CreateStaffBody request, CancellationToken token) =>
       {
           if (string.IsNullOrEmpty(request.Email)) return Results.NoContent();
           if (string.IsNullOrEmpty(request.FirstName)) return Results.NoContent();
           if (string.IsNullOrEmpty(request.LastName)) return Results.NoContent();
           if (string.IsNullOrEmpty(request.Phone)) return Results.NoContent();
           if (string.IsNullOrEmpty(request.Role.ToString())) return Results.NoContent();

           var staff = await service.CreateAsync(request, token);

           return Results.Created($"/api/staff/{staff.Id}", staff);
       });

        apiGroup.MapPut("/{id}", async (Guid id, IStaffService service, UpdateStaffBody request, CancellationToken token) =>
        {
            if (string.IsNullOrEmpty(request.Email)) return Results.NoContent();
            if (string.IsNullOrEmpty(request.FirstName)) return Results.NoContent();
            if (string.IsNullOrEmpty(request.LastName)) return Results.NoContent();
            if (string.IsNullOrEmpty(request.Phone)) return Results.NoContent();
            if (string.IsNullOrEmpty(request.Role.ToString())) return Results.NoContent();

            var staff = await service.UpdateAsync(id, request, token);

            return Results.Ok(staff);
        });

        apiGroup.MapDelete("/{id}", async (Guid id, IStaffService service, CancellationToken token) =>
        {
            await service.DeleteAsync(id, token);
            return Results.NoContent();
        });
    }
}