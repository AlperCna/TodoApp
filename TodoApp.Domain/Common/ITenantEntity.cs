namespace TodoApp.Domain.Common;

public interface ITenantEntity
{
    public Guid TenantId { get; set; }
}