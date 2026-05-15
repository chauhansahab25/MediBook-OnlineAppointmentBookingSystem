using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProviderService.Data;
using ProviderService.Repositories;
using ProviderService.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IProviderService, ProviderService.Services.ProviderService>();

// ── HTTP Clients ─────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthServiceUrl"] ?? "http://localhost:5219");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("ReviewService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ReviewServiceUrl"] ?? "http://localhost:5211");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// ── JWT Authentication ──────────────────────────────────────────────────────
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing from configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// ── CORS ────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

});


// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MediBook Provider Service",
        Version = "v1",
        Description = "Manages healthcare provider profiles, verification, and availability."
    });
});

var app = builder.Build();

// ── Middleware ──────────────────────────────────────────────────────────
app.UseRouting();
app.UseCors("AllowAll");

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediBook Provider Service v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => "MediBook Provider Service Running ✅");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ProviderService" }));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();