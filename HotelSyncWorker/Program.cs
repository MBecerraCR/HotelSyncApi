using HotelSyncWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OperaPollingWorker>();

var host = builder.Build();
host.Run();
