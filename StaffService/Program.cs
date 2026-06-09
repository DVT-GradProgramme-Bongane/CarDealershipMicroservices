using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StaffDBContext>(options => options.UseNpgsql(PostgresConnectionString.Build(builder.Configuration))); 
builder.Services.AddScoped<IStaffService, StaffServiceController>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002); // REST endpoints + api gateway
    options.ListenAnyIP(5102, listen =>
    {
        listen.Protocols = HttpProtocols.Http2; // for grpc 
    });
});

builder.Services.AddGrpc();


var app = builder.Build();

// table migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StaffDBContext>();
    await db.Database.MigrateAsync();
}

app.MapGrpcService<GrpcStaffService>();
app.MapStaffEndpoints();
app.Run();
