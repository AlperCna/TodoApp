using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Security;

public interface IJwtTokenService
{
    // Kullanıcı bilgilerini alarak ona özel bir JWT bilet üretir
    string CreateToken(User user);
}