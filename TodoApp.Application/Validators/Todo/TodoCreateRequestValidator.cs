using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using TodoApp.Application.DTOs.Todo;
using Ganss.Xss;

namespace TodoApp.Application.Validators.Todo;

public class TodoCreateRequestValidator : AbstractValidator<TodoCreateRequest>
{
    public TodoCreateRequestValidator()
    {
        var sanitizer = new HtmlSanitizer();

        // 1. Başlık Validasyonları
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Görev başlığı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Başlık en az 3 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.")
            // XSS Koruması (Hocanın istediği SRP uyumu)
            .Must(title => title == sanitizer.Sanitize(title))
            .WithMessage("Başlık geçersiz karakterler veya zararlı kodlar içeriyor.");

        // 2. Açıklama Validasyonları (Opsiyonel)
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.")
            .Must(desc => string.IsNullOrEmpty(desc) || desc == sanitizer.Sanitize(desc))
            .WithMessage("Açıklama alanı zararlı HTML içeremez.");

        // 3. Tarih Validasyonları
        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Teslim tarihi belirtilmelidir.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Teslim tarihi geçmiş bir zaman olamaz.");
    }
}