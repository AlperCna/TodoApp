using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.Interfaces.Security;

public interface IPasswordHasher
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
}
