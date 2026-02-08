using System.Net;
using System.Text.Json;

namespace TodoApp.WebApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // İsteği bir sonraki adıma ilet
            await _next(context);
        }
        catch (Exception ex)
        {
            // Hata oluşursa yakala ve işle
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 1. Geliştirme: Hata tipine göre durum kodu ve mesaj eşleşmesi yapıyoruz
        var (statusCode, message) = exception switch
        {
            // 401: Token yoksa veya geçersizse
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim."),

            // 400: Email zaten kayıtlı veya geçersiz bir işlem yapılmaya çalışıldığında
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),

            // 400: Parametreler hatalı gelirse
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),

            // 404: Olmayan bir Todo ID'si ile işlem yapılırsa
            KeyNotFoundException => (HttpStatusCode.NotFound, "İstenen kayıt bulunamadı."),

            // 500: Beklenmeyen sistem hataları
            _ => (HttpStatusCode.InternalServerError, "Sunucu tarafında bir hata oluştu.")
        };

        context.Response.StatusCode = (int)statusCode;

        // 2. Geliştirme: Daha profesyonel ve scannable bir response modeli
        var response = new
        {
            status = context.Response.StatusCode,
            message = message,
            // Geliştirme aşamasında hatayı görmek için (opsiyonel):
            // detail = exception.StackTrace 
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}