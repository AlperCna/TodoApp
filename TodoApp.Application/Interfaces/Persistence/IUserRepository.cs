using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);

    // Refresh Token ile kullanıcıyı bulmak için (Yeni eklendi)
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    // Kullanıcı bilgilerini (Tokenlar vb.) güncellemek için (Yeni eklendi)
    Task UpdateAsync(User user, CancellationToken ct = default);
}