using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Interfaces.Security;
using TodoApp.Application.Services.Auth;
using TodoApp.Application.Services.Todo;
using TodoApp.Application.Services.Admin;
using TodoApp.Infrastructure.Persistence;
using TodoApp.Infrastructure.Persistence.Repositories;
using TodoApp.Infrastructure.Security;
using TodoApp.WebApi.Services;
using TodoApp.WebApi.Middlewares;
using TodoApp.Infrastructure.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

var builder = WebApplication.CreateBuilder(args);

// --- 1. VERİTABANI BAĞLANTISI (EF CORE) ---
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ EKLEME: SSO Correlation Hatasını Önlemek İçin Çerez Politikası
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.CheckConsentNeeded = context => true;
});

// --- 2. GÜVENLİK VE JWT + SSO (AZURE & GOOGLE) AYARLARI ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddMicrosoftAccount("Microsoft", options =>
{
    options.ClientId = builder.Configuration["AzureAd:ClientId"]!;
    options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"]!;
    options.CallbackPath = builder.Configuration["AzureAd:CallbackPath"];
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
})
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
});

builder.Services.AddAuthorization();

// --- 3. CORS AYARLARI ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
    });
});

// --- 4. VALIDASYON KATMANI (FLUENT VALIDATION) ---
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ITodoService>();

// --- 5. HANGFIRE YAPILANDIRMASI ---
builder.Services.AddHangfire(config => config
  .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
  .UseSimpleAssemblyNameTypeSerializer()
  .UseRecommendedSerializerSettings()
  .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// --- 6. DEPENDENCY INJECTION ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<TodoReminderJob>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddControllers();

// --- 7. SWAGGER AYARLARI (✅ DÜZELTİLDİ) ---
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

    // ✨ Buradaki parantez hatasını düzelttik:
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
      Array.Empty<string>()
    }
  });
});

var app = builder.Build();

// --- 8. HTTP REQUEST PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHangfireDashboard();

// CSP Ayarı
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    await next();
});

app.UseHttpsRedirection();

// ✅ EKLEME: Çerez politikasını devreye al (Authentication'dan önce olmalı)
app.UseCookiePolicy();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- 10. RECURRING JOBS ---
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<TodoReminderJob>(
      "todo-reminder-job",
      job => job.SendRemindersAsync(),
      Cron.Hourly);
}

app.Run();