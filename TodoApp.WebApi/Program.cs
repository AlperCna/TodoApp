using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Interfaces.Security;
using TodoApp.Application.Services.Auth;
using TodoApp.Application.Services.Todo;
using TodoApp.Infrastructure.Persistence;
using TodoApp.Infrastructure.Persistence.Repositories;
using TodoApp.Infrastructure.Security;
using TodoApp.WebApi.Services;
using TodoApp.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI BAĞLANTISI (EF CORE) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. GÜVENLİK VE JWT AYARLARI ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});

builder.Services.AddAuthorization();

// --- 3. CORS AYARLARI ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- 4. DEPENDENCY INJECTION (Kabloları Bağlama) ---
builder.Services.AddHttpContextAccessor(); // HttpContext erişimi için

// ✅ Multi-Tenancy ve Altyapı Servisleri
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>(); // 👈 KRİTİK: Dinamik Şirket Yönetimi için eklendi
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ✅ Todo Katmanı
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddControllers();

// --- 5. SWAGGER AYARLARI (JWT DESTEKLİ) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoApp API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Örn: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 6. HTTP REQUEST PIPELINE (Sıralama Hayatidir!) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Hata Yakalayıcı (ExceptionMiddleware)
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Kimlik Doğrulama ve Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();