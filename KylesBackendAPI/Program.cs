var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

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

app.MapControllers();

app.Run();