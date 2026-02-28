using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Exceptions;

namespace TodoApp.WebApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uygulama Hatası: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 1. Durum Kodu ve Başlık Belirleme
        var (statusCode, title, type) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Doğrulama Hatası", "ValidationError"),
            BusinessException => (HttpStatusCode.UnprocessableEntity, "İş Kuralı İhlali", "BusinessError"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz Erişim", "AuthError"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Kayıt Bulunamadı", "NotFoundError"),
            _ => (HttpStatusCode.InternalServerError, "Sunucu Hatası", "ServerError")
        };

        context.Response.StatusCode = (int)statusCode;

        // 2. ProblemDetails Nesnesini Oluşturma
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Type = type,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        // 3. ÖZEL VERİLERİ EKLEME 

        // Eğer hata bir BaseException ise ErrorCode ekle
        if (exception is BaseException baseEx && !string.IsNullOrEmpty(baseEx.ErrorCode))
        {
            problem.Extensions.Add("errorCode", baseEx.ErrorCode);
        }

        // Eğer hata bir BusinessException ise ek verileri (AdditionalData) ekle
        if (exception is BusinessException busEx && busEx.AdditionalData != null)
        {
            problem.Extensions.Add("data", busEx.AdditionalData);
        }

        // Validasyon hatalarını (liste olarak) ekle
        if (exception is ValidationException valEx)
        {
            problem.Extensions.Add("errors", valEx.Errors);
        }

        // Development ortamındaysak StackTrace ekle
        if (_env.IsDevelopment())
        {
            problem.Extensions.Add("stackTrace", exception.StackTrace);
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }
}