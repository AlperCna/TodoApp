using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TodoApp.Application.Interfaces.Persistence/ITenantRepository.cs
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface ITenantRepository
{
    Task<List<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);

}