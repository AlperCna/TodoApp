using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation.Results;

namespace TodoApp.Application.Exceptions;

public class ValidationException : Exception
{
    // Hangi alanın (property) hangi hataları (error) olduğunu tutan sözlük
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Bir veya daha fazla validasyon hatası oluştu.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    // FluentValidation'dan gelen hataları otomatik olarak sözlüğe çeviren constructor
    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.ToArray()
            );
    }
}