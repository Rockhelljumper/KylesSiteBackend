using KylesBackendAPI.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

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

// Add CORS services with policy allowing localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalhostPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", // For React default port
                           "http://localhost:4200", // For Angular default port
                           "http://localhost:5173", // For Vite default port
                           "http://localhost:8080", // Common alternative port
                           "http://127.0.0.1:5500") // For Live Server in VS Code
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

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

// Use CORS middleware - place before routing or authorization
app.UseCors("LocalhostPolicy");

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