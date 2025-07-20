using InventoryAPI.Data;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddHttpLogging(); 

// Build connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databasePassword = builder.Configuration["DatabasePassword"];

if (string.IsNullOrEmpty(databasePassword))
{
    throw new InvalidOperationException("Database password is not configured.");
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

connectionString = connectionString.Replace("{DatabasePassword}", databasePassword);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register custom services
builder.Services.AddScoped<IStockLogService, StockLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecretKey = jwtSettings["SecretKey"];
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 7001;
});

// Build App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpLogging();
}
else
{
    app.UseHttpsRedirection();
}

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

// Exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Jwt authentication
app.UseMiddleware<JwtFromCookieMiddleware>();

// Cors
app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();