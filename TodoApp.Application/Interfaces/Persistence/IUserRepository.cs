using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface IUserRepository
{
    // --- Standart Kullanıcı İşlemleri ---
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<User?> GetByExternalIdAsync(string externalId, string provider, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);

    // --- ✨ YENİ: Refresh Token Yönetimi (Ayrı Tablo Mantığı) ---

    // Yeni bir refresh token kaydetmek için
    Task AddRefreshTokenAsync(UserRefreshToken token, CancellationToken ct = default);

    // Token üzerinden kullanıcıya ulaşmak için (Eski GetByRefreshTokenAsync yerine)
    // Bu metod UserRefreshToken döner, içindeki .User property'si ile kullanıcıya erişiriz.
    Task<UserRefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);

    // Bir token'ı güncellemek (Örn: IsRevoked = true yapmak için)
    Task UpdateRefreshTokenAsync(UserRefreshToken token, CancellationToken ct = default);

    // Güvenlik için: Kullanıcının tüm eski tokenlarını iptal etmek için
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);
}