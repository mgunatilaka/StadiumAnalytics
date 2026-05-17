using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StadiumAnalytics.Infrastructure.Data;
using StadiumAnalytics.Infrastructure.Messaging;
using StadiumAnalytics.Application.Services;
using StadiumAnalytics.Application.Workers;
using StadiumAnalytics.Domain.Interface.Messaging;
using FluentValidation;
using StadiumAnalytics.Application.Validators;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Configure OpenTelemetry Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("StadiumAnalytics.Api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddConsoleExporter();
    });

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    // System.Text.Json uses ISO 8601 by default
    options.JsonSerializerOptions.WriteIndented = true;
});

// Add Health Checks
builder.Services.AddHealthChecks();


// Configure OpenAPI for Scalar
//builder.Services.AddOpenApi();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(DateTimeOffset))
        {
            schema.Type = Microsoft.OpenApi.JsonSchemaType.String;
            schema.Format = "date-time";
        }
        return Task.CompletedTask;
    });
});

// Setup Database (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=stadium.db"));

// Register messaging channel (Singleton)
builder.Services.AddSingleton<ISensorEventChannel,SensorEventChannel>();

// Register Application Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Application repository
builder.Services.AddScoped<ISensorEventRepository, SensorEventRepository>();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<SensorEventCreateDtoValidator>();

// Register Background Workers
builder.Services.AddHostedService<EventConsumerWorker>();


var app = builder.Build();

// Auto-create database for the sake of the challenge (in production, use migrations)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Stadium Analytics API");
       
    });
}

//TODO: Add Authentication and Authorization for production (e.g., JWT, API Keys, or OAuth2)
//TODO: add global exception handling middleware for better error responses and logging
//TODO: consider adding CORS policy for production
//TODO: add rate limiting for production to prevent abuse
//TODO: consider adding caching for analytics endpoints in production for better performance
//TODO: add more comprehensive logging (e.g., correlation IDs, structured logging) for better observability in production
//TODO: add API request and response logging for better debugging and monitoring in production



app.UseHttpsRedirection();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
