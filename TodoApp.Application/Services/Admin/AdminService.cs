using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs;
using TodoApp.Application.Interfaces.Persistence;

namespace TodoApp.Application.Services.Admin;

public class AdminService : IAdminService
{
    private readonly IUserRepository _users;

    // Repository enjeksiyonu (Infrastructure katmanıyla iletişim bu repo üzerinden sağlanır)
    public AdminService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<List<UserSummaryDto>> GetUsersSummaryAsync(CancellationToken ct = default)
    {
        // 1. Repository üzerinden ham veritabanı entity listesini alıyoruz.
        // Veritabanı erişim mantığı Infrastructure/Persistence/Repositories/UserRepository.cs içindedir.
        var userEntities = await _users.GetAllAsync(ct);

        // 2. Ham entity'leri, Controller'ın beklediği DTO yapısına map'liyoruz.
        // Bu sayede veritabanı şemasını dış dünyadan izole etmiş oluyoruz.
        return userEntities.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}