using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.Interfaces.Common;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
}