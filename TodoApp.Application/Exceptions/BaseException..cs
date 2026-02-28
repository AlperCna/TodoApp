using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.Exceptions;

public abstract class BaseException : Exception
{
    public string? ErrorCode { get; }

    protected BaseException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }
}