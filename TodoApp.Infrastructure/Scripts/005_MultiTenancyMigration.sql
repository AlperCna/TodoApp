-- 1. TENANTS TABLOSUNU OLUŞTURMA
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE [Tenants] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
    );
END
GO -- Tablo oluşturma işlemini bitir

-- 2. SÜTUNLARI EKLEME
ALTER TABLE [Users] ADD [TenantId] UNIQUEIDENTIFIER NULL;
ALTER TABLE [TodoItems] ADD [TenantId] UNIQUEIDENTIFIER NULL;
GO -- Sütun ekleme işlemini bitir (Artık SQL bu sütunları tanıyor)

-- 3. VARSAYILAN TENANT OLUŞTURMA VE VERİLERİ BAĞLAMA
DECLARE @DefaultTenantId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [Tenants] ([Id], [Name], [CreatedAt])
VALUES (@DefaultTenantId, 'TestTenant1', GETUTCDATE());

-- Artık bu sütunlar bilindiği için hata almazsın
UPDATE [Users] SET [TenantId] = @DefaultTenantId;
UPDATE [TodoItems] SET [TenantId] = @DefaultTenantId;
GO

-- 4. KISITLAMALARI (CONSTRAINT) EKLEME
ALTER TABLE [Users] ALTER COLUMN [TenantId] UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE [TodoItems] ALTER COLUMN [TenantId] UNIQUEIDENTIFIER NOT NULL;

ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Tenants_TenantId] 
    FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);

ALTER TABLE [TodoItems] ADD CONSTRAINT [FK_TodoItems_Tenants_TenantId] 
    FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);
GO