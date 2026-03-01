// Gerekli kütüphanelerin olduğundan emin ol
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Persistence;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        // Kayıt ekranında tüm şirketleri listelemek için filtreyi kapatıyoruz
        return await _context.Tenants
            .IgnoreQueryFilters()
            .ToListAsync(ct);
    }

    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Name == name, ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
        await _context.SaveChangesAsync(ct);
    }

    // Yeni eklenen: Domain uzerinden sirketi bulmak icin
    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters() // Sistem genelinde alan adi eslesmesi aranmali
            .FirstOrDefaultAsync(t => t.Domain == domain, ct);
    }
}