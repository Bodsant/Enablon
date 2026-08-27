using Ehsms.Worker;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ArchitectureWorker>();
await builder.Build().RunAsync();
