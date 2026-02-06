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
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Hataya göre HTTP kodunu belirle
        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            _ => (int)HttpStatusCode.InternalServerError // 500
        };

        var response = new { message = exception.Message };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}