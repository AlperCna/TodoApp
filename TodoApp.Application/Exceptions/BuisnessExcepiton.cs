using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.Exceptions;

public class BusinessException : BaseException
{
    // Hata anındaki verileri (örn: aşılmaya çalışılan limit) tutar
    public object? AdditionalData { get; }

    public BusinessException(string message, string? errorCode = "BUSINESS_ERROR", object? additionalData = null)
        : base(message, errorCode)
    {
        AdditionalData = additionalData;
    }
}