using avans_1._4_ai_system_integration_api.Mapping;
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using avans_1_4_ai_system_integration_api.Data;
using avans_1_4_ai_system_integration_api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register MVC controllers for handling HTTP requests.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// Register a global exception handler middleware to catch and handle unhandled exceptions gracefully.
builder.Services.AddExceptionHandler<TrashDetectionExceptionHandler>();
builder.Services.AddProblemDetails();

// Register OpenAPI/Swagger for API documentation and testing.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Trash Detection API",
        Version = "v1"
    });
});


builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Register authorization services for securing endpoints.
builder.Services.AddAuthorization();

// Retrieve the SQL connection string from configuration.
var sqlConnectionString = builder.Configuration.GetConnectionString("SqlConnectionString")
    ?? builder.Configuration.GetValue<string>("SqlConnectionString");
if (string.IsNullOrWhiteSpace(sqlConnectionString))
{
    throw new InvalidOperationException("Configuration value 'SqlConnectionString' is missing or empty.");
}
// Register the EF database context with the specified SQL connection string.
builder.Services.AddDbContext<TrashDetectionDbContext>(options => options.UseSqlServer(sqlConnectionString));


// Register ASP.NET Core Identity with entity framework stores and configure password and user requirements.
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<TrashDetectionDbContext>();

// Register IHttpContextAccessor for accessing HTTP context in services (e.g., to get current user info).
builder.Services.AddHttpContextAccessor();

// Register services for handling user account operations
builder.Services.AddTransient<IUserMappingService, UserMappingService>();
builder.Services.AddTransient<IAccountService, AccountService>();


// HttpClient for the sensor API
builder.Services.AddHttpClient<ISensorApiClient, SensorApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SensorApi:BaseUrl"]!);
});

// Repository + Service
builder.Services.AddScoped<ITrashDetectionRepository, TrashDetectionRepository>();
builder.Services.AddScoped<ITrashDetectionService, TrashDetectionService>();

// Register an HttpClient for communicating with the external sensor API, with the base URL configured from app settings.
builder.Services.AddHttpClient<ISensorApiClient, SensorApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SensorApi:BaseUrl"]!);
});

// Register the repository and service for handling trash detection data, with scoped lifetimes to ensure a new instance per request.
builder.Services.AddScoped<ITrashDetectionRepository, TrashDetectionRepository>();
builder.Services.AddScoped<ITrashDetectionService, TrashDetectionService>();


var app = builder.Build();

// Apply any pending database migrations on startup to ensure the database schema is up to date.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrashDetectionDbContext>();
    db.Database.Migrate();
}

// Register OpenAPI/Swagger endpoints.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Trash Detection API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
        options.CacheLifetime = TimeSpan.Zero; // Disable caching for development
    });
}
else
{
    // Show the health message directly in non-development environments
    var buildTimeStamp = File.GetCreationTime(Assembly.GetExecutingAssembly().Location);
    var currentHealthMessage = $"The Trash Detection API is up 🚀 | Build timestamp: {buildTimeStamp}";

    app.MapGet("/", () => currentHealthMessage);
}


// Enforce HTTPS for all requests.
app.UseHttpsRedirection();

// Use the global exception handler middleware to catch and handle unhandled exceptions gracefully.
app.UseExceptionHandler();

// Enable authentication middleware.
app.UseAuthentication();

// Enable authorization middleware.
app.UseAuthorization();

// Register all controller endpoints for the application.
app.MapControllers();

// Built-in token endpoints on a separate route group
app.MapGroup("/api/identity")
   .MapIdentityApi<User>();

app.Run();
