using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shipping.Api.Middleware;
using Shipping.Application.Commands;
using Shipping.Application.Interfaces;
using Shipping.Application.Validators;
using Shipping.Domain.Services.Pricing;
using Shipping.Infrastructure.Persistence;
using Shipping.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Structured Logging
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .Enrich.FromLogContext());

// Add Services to Container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Logistics Shipping Platform API",
        Version = "v1",
        Description = "Enterprise Shipping Platform API implementing Clean Architecture, CQRS, DDD, and Gherkin Spec Driven Engineering."
    });
});

// Configure Database (EF Core PostgreSQL with InMemory fallback if connection string not provided)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=shipping_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ShippingDbContext>(options =>
{
    if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        options.UseInMemoryDatabase("ShippingDbInMemory");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// Register Domain & Application Services
builder.Services.AddScoped<IShippingCostCalculator, ShippingCostCalculator>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Register MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerCommandValidator>();

// Health Checks
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure Middleware Pipeline
app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipping Platform API v1"));
}

app.UseCors("AllowFrontend");
app.UseAuthorization();

// Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.MapControllers();

// Ensure DB Created (Development mode)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// Make Program class accessible to WebApplicationFactory integration tests
public partial class Program { }
