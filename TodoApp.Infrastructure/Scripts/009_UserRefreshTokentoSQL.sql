USE [TodoAppDB];
GO

-- 1. Users tablosundaki eski kolonları temizleyelim (Koddan sildik, DB'den de silinmeli)
ALTER TABLE [Users] DROP COLUMN [RefreshToken];
ALTER TABLE [Users] DROP COLUMN [RefreshTokenExpiryTime];



USE [TodoAppDB];
GO

-- 2. Yeni Refresh Token tablosunu oluşturalım
CREATE TABLE [UserRefreshTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Token] NVARCHAR(500) NOT NULL,
    [ExpiryTime] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedByIp] NVARCHAR(50) NULL,
    [IsRevoked] BIT NOT NULL,
    [RevokedAt] DATETIME2 NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,

    -- Foreign Key: Bu token hangi kullanıcıya ait?
    CONSTRAINT [FK_UserRefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) 
    REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

-- 3. Performans için UserId üzerine index atalım (Sık sorgulanacak)
CREATE INDEX [IX_UserRefreshTokens_UserId] ON [UserRefreshTokens] ([UserId]);