using GarageFlow.Api.Data;
using GarageFlow.Api.Middleware;
using GarageFlow.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/garageflow-api-.log", rollingInterval: RollingInterval.Day);
});

// PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? "Host=localhost;Port=5432;Database=garageflow;Username=garageflow_user;Password=change-me";
builder.Services.AddDbContext<ApiDbContext>(options => options.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<SyncProcessor>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// API key middleware
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

// Health endpoint (no auth)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
