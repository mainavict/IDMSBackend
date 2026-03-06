using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using  Microsoft.EntityFrameworkCore;
using IDMSBackend.Data;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.Services.Implementations;
using System.Text.Json.Serialization;
using IDMSBackend.BackgroundServices;

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
builder.Services.AddControllers() .AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));;

// This adds the OpenAPI (Swagger) document generation
builder.Services.AddOpenApi(); 

var syncSettings = builder.Configuration.GetSection("SyncSettings");
var baseUrl = syncSettings["UniversityBaseUrl"];

builder.Services.AddHttpClient("UniversityApi", client =>
{
    client.BaseAddress = new Uri(baseUrl??"https://default-url.com"); // endpint  address
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(30); // Give it 30 minutes to download 5,000+ records
});


// Future: builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IStudentCards, StudentCardServices>();
builder.Services.AddScoped<IUserService, UserServices>();
builder.Services.AddScoped<IRoleServices, RoleServices>();
builder.Services.AddScoped<IStudentServices, StudentServices>();
builder.Services.AddHostedService<StudentSyncWorker>();
builder.Services.AddScoped<IDomainsServices, DomainServices>();
builder.Services.AddScoped<IUserDomainRole, UserDomainRoleServices>();

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
    
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    DbInitializer.Seed(context);
    
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