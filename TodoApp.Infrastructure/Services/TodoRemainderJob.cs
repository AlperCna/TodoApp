using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.Infrastructure.Services;

public class TodoReminderJob
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public TodoReminderJob(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendRemindersAsync()
    {
        var now = DateTime.UtcNow;
        var tomorrow = now.AddDays(1);

        // 1. Görevleri filtreleyerek çekiyoruz
        var pendingTodos = await _context.TodoItems
            .IgnoreQueryFilters() // Multi-tenancy engelini aşar
            .Include(t => t.User)
            .Where(t => !t.IsCompleted &&
                        !t.IsReminderSent &&
                        t.DueDate >= now &&
                        t.DueDate <= tomorrow)
            .ToListAsync();

        Console.WriteLine($"[ReminderJob] {pendingTodos.Count} adet hatırlatılacak görev bulundu.");

        foreach (var todo in pendingTodos)
        {
            if (todo.User != null && !string.IsNullOrEmpty(todo.User.Email))
            {
                try
                {
                    string subject = $"Hatırlatma: {todo.Title}";
                    string body = $@"<h3>Merhaba {todo.User.UserName},</h3>
                                    <p><strong>{todo.Title}</strong> başlıklı görevinizin vakti geliyor!</p>
                                    <p>Teslim Tarihi: {todo.DueDate:dd.MM.yyyy HH:mm}</p>";

                    // 2. Maili gönderiyoruz
                    await _emailService.SendEmailAsync(todo.User.Email, subject, body);

                    // 3. Veritabanını anlık güncelliyoruz (Spam engeli)
                    todo.IsReminderSent = true;
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[ReminderJob] '{todo.Title}' başarıyla gönderildi.");

                    // 4. KRİTİK: Mailtrap hız limitine takılmamak için 2 saniye bekle
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReminderJob] HATA: '{todo.Title}' gönderilemedi: {ex.Message}");
                }
            }
        }
    }
}