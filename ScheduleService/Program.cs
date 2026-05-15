using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScheduleService.Data;
using ScheduleService.Repositories;
using ScheduleService.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── HTTP Client for Provider-Service communication ───────────────────────────
builder.Services.AddHttpClient("ProviderService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:ProviderService"]
        ?? "http://localhost:5096");

});

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<IScheduleService, ScheduleService.Services.ScheduleService>();

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
        Title = "MediBook Schedule Service",
        Version = "v1",
        Description = "Manages provider availability slots, booking states, and recurring schedules."
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediBook Schedule Service v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => "MediBook Schedule Service Running ✅");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ScheduleService" }));

app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate on Startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();