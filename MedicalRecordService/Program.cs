using MedicalRecordService.Data;
using MedicalRecordService.Repositories;
using MedicalRecordService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IRecordRepository, RecordRepository>();
builder.Services.AddScoped<IRecordService, RecordService>();

// ── HTTP Clients for Inter-Service Communication ──────────────────────────────
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AuthService"] ?? "http://localhost:5219");
});

builder.Services.AddHttpClient("ProviderService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProviderService"] ?? "http://localhost:5096");
});

builder.Services.AddHttpClient("AppointmentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AppointmentService"] ?? "http://localhost:5177");
});


// ── Background Service (Follow-Up Reminders) ─────────────────────────────────
// FollowUpReminderService removed - notification service dependency

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
        Title = "MediBook Medical Record Service",
        Version = "v1",
        Description = "Manages encrypted electronic health records with AES-256 and automated follow-up reminders."
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json",
        "MediBook Medical Record Service v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => "MediBook Medical Record Service Running ✅");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "MedicalRecordService" }));

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate on Startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();