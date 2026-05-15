using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ReviewService.Data;
using ReviewService.Repositories;
using ReviewService.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── HTTP Clients ─────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("ProviderService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:ProviderService"]
        ?? "http://localhost:5096");
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:AuthService"]
        ?? "http://localhost:5219");
});

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService.Services.ReviewService>();

// ── CORS ────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
        Title = "MediBook Review Service",
        Version = "v1",
        Description = "Manages patient reviews and ratings for healthcare providers."
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediBook Review Service v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => "MediBook Review Service Running ✅");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ReviewService" }));


app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate on Startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();