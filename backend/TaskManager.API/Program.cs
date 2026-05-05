using TaskManager.API.Extensions;
using TaskManager.API.Middleware;
using TaskManager.Application.Extensions;
using TaskManager.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("SWAGGER_ENABLED"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// For integration tests
public partial class Program { }
