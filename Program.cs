using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using  Microsoft.EntityFrameworkCore;
using IDMSBackend.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 1. Add Services to the Container ---
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
        name: "PostgreSQL", 
        failureStatus: HealthStatus.Unhealthy, 
        tags: new[] { "db", "data" });
builder.Services.AddControllers();

// This adds the OpenAPI (Swagger) document generation
builder.Services.AddOpenApi(); 

// Future: builder.Services.AddDbContext<AppDbContext>(...);
// Future: builder.Services.AddScoped<IIdentityService, IdentityService>();

var app = builder.Build();

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
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            }),
            duration = report.TotalDuration
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

//Scaler API Reference setup - only in development for now
if (app.Environment.IsDevelopment())
{
    
    app.MapOpenApi();
    
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("IDMS Backend API")
            .WithTheme(ScalarTheme.Moon) // Try 'Purple', 'Solarized', or 'DeepSpace'
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();