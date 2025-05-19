using KylesBackendAPI.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "Kyle's Backend API",
        Description = "An ASP.NET Core Web API for managing Kyle's portfolio backend services",
        Contact = new OpenApiContact
        {
            Name = "Kyle Simmons",
            Email = "kyle7simmons1994@gmail.com",
            Url = new Uri("https://kylesimmons.tech")
        },
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("system", () => HealthCheckResult.Healthy("System is healthy"))
    .AddCheck<DatabaseHealthCheck>("database");

// CORS Configuration - Most permissive for troubleshooting
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true); // Redundant with AllowAnyOrigin but added for emphasis
    });
});

var app = builder.Build();

// Ensure Resume directory exists
string resumeDirectory = app.Configuration["ResumeDirectory"] ??
    Path.Combine(app.Environment.ContentRootPath, "Resumes");

app.Logger.LogInformation("Application startup - Resume directory path: {ResumeDirectory}", resumeDirectory);

if (!Directory.Exists(resumeDirectory))
{
    try
    {
        Directory.CreateDirectory(resumeDirectory);
        app.Logger.LogInformation("Application startup - Created directory: {ResumeDirectory}", resumeDirectory);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Application startup - Failed to create directory: {ResumeDirectory}", resumeDirectory);
    }
}

// Apply CORS globally via middleware - MUST be before other middleware
app.UseCors("AllowAll");

// Additional middleware to explicitly add CORS headers to all responses
app.Use(async (context, next) =>
{
    // Add CORS headers to ensure they're present even if normal CORS middleware doesn't handle it
    context.Response.OnStarting(() =>
    {
        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        }

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Methods"))
        {
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        }

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
        {
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        }

        return Task.CompletedTask;
    });

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Disable HTTPS redirection in development
    // This allows HTTP calls from localhost
}
else
{
    // Only use HTTPS redirection in production
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Add health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapControllers();

app.Run();