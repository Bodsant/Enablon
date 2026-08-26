using Ehsms.BuildingBlocks;
using Ehsms.Modules.Platform.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// === Serilog ===
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// === Database ===
builder.Services.AddDbContext<EhsmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly(typeof(EhsmsDbContext).Assembly.FullName)));

// === Building Blocks ===
builder.Services.AddSingleton<IClock, UtcClock>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EhsmsDbContext>());

// === MediatR ===
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EhsmsDbContext).Assembly));

// === Controllers ===
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EHSMS API", Version = "v1" });
});

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// === Health Checks ===
builder.Services.AddHealthChecks();

var app = builder.Build();

// === Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// === Auto-migrate in Development ===
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EhsmsDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
