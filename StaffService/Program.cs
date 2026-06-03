using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StaffDBContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("StaffDb"))); // add env connection string
builder.Services.AddScoped<IStaffService, StaffServiceController>();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GrpcStaffService>();



app.Run();
