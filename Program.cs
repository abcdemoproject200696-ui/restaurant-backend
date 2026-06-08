using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Cloud (Render) PORT env var pe suno; warna local 5000
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ===== Services =====

// Database: connection string Postgres jaisा ho (cloud/Neon) to Npgsql,
// warna local SQLite file. Render pe env var "ConnectionStrings__Default" set hoga.
var connStr = builder.Configuration.GetConnectionString("Default") ?? "Data Source=restaurant.db";
var isPostgres = connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                 || connStr.Contains("postgres", StringComparison.OrdinalIgnoreCase);
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (isPostgres) opt.UseNpgsql(connStr);
    else opt.UseSqlite(connStr);
});

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    // enum ko number ki jagah string me bhejo: "Pending", "Delivered" etc.
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Order -> Items -> Order wale circular loop se bacho
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger UI ke liye (API test/check karne wala interactive page)
builder.Services.AddSwaggerGen();

// Razorpay API call ke liye HttpClient
builder.Services.AddHttpClient();

// OTP store (ek hi instance poore app me)
builder.Services.AddSingleton<OtpService>();

// CORS — Angular/Ionic dev server se request allow karne ke liye
const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ===== DB create + seed =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
    DbSeeder.SeedUsers(db);
    DbSeeder.SeedDiscounts(db);
}

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Swagger UI -> http://localhost:5000/swagger  (yahan saare API test kar sakte ho)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors(CorsPolicy);
app.MapControllers();

// Simple health check / welcome
app.MapGet("/", () => "Resturent-MobileApp-Backend is running. Try /api/menu");

app.Run();
