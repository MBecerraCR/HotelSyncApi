using HotelSyncApi.Data;
using HotelSyncApi.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<SyncRepository>();
builder.Services.AddHttpClient<HubSpotService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<OperaCloudService>();

builder.Services.AddScoped<SyncEngine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
