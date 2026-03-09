using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence.Configirations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        // 1. Tablo ismi
        builder.ToTable("UserRefreshTokens");

        // 2. Birincil anahtar
        builder.HasKey(x => x.Id);

        // 3. Özellik (Property) Yapılandırmaları
        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ExpiryTime)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(50);

        // KRİTİK NOKTA: Veritabanındaki DEFAULT(0) ile tam uyum sağlıyoruz.
        // ValueGeneratedOnAdd: "Ben değer göndermezsem SQL sen hallet" demek.
        builder.Property(x => x.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RevokedAt)
            .IsRequired(false); // Boş kalabilir (NULL allowed)

        // İlişki Yapılandırması (Foreign Key)
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index: Token aramaları çok sık yapılacağı için buraya da bir index ekleyelim
        builder.HasIndex(x => x.Token)
            .IsUnique(); // Aynı token iki kere oluşamaz

        // Performans için UserId aramalarına index
        builder.HasIndex(x => x.UserId);
    }
}