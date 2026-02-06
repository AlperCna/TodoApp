using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.Interfaces.Common;

public interface ICurrentUserService
{
    // Token içerisinden o anki kullanıcının ID'sini Guid olarak döndürür
    // Veritabanında 'uniqueidentifier' kullandığımız için Guid? tipini seçtik.
    Guid? UserId { get; }
}