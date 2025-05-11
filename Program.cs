using apbd_cw9.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();