using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StaffDBContext>(options => options.UseNpgsql(PostgresConnectionString.Build(builder.Configuration))); 
builder.Services.AddScoped<IStaffService, StaffServiceController>();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GrpcStaffService>();


app.MapStaffEndpoints();
app.Run();
