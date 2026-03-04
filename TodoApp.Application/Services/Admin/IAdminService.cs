using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs;

namespace TodoApp.Application.Services.Admin;

public interface IAdminService
{
    Task<List<UserSummaryDto>> GetUsersSummaryAsync(CancellationToken ct = default);
}