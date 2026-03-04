using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Domain.Entities;

public class UserRefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Güvenlik anahtarımız
    public string Token { get; set; } = default!;

    // Token'ın ne zaman geçersiz olacağı
    public DateTime ExpiryTime { get; set; }

    // Token'ın oluşturulma tarihi
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 🛡️ Hocanın istediği "Geçmiş/Audit" bilgileri
    public string? CreatedByIp { get; set; }

    // Token iptal edildi mi? (Örn: Logout yapınca veya çalınma şüphesinde true olur)
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    // İlişki Bilgisi: Bu token hangi kullanıcıya ait?
    public Guid UserId { get; set; }

    // Navigation Property: EF Core için kullanıcı bağlantısı
    public User User { get; set; } = default!;

    // 💡 Yardımcı metot: Token'ın süresi geçti mi veya iptal mi edildi?
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryTime;
}