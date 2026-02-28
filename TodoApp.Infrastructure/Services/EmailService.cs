using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using TodoApp.Application.Interfaces.Common;

namespace TodoApp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration) { _configuration = configuration; }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TodoApp", _configuration["EmailSettings:From"]));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        try
        {
            // Mailtrap için StartTls ve Port 587/2525 en güvenlisidir
            await client.ConnectAsync(_configuration["EmailSettings:Host"],
                                      int.Parse(_configuration["EmailSettings:Port"]!),
                                      SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_configuration["EmailSettings:UserName"],
                                           _configuration["EmailSettings:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception)
        {
            throw; // Hatayı Job'a fırlat ki orada loglansın
        }
    }
}