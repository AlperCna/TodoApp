USE [TodoAppDB];
GO

-- 1. En kritik sorgu: Kullanıcıya özel, silinmemiş ve tarihe göre sıralı veri
CREATE INDEX IX_TodoItems_UserId_IsDeleted_CreatedAt 
ON dbo.TodoItems (UserId, IsDeleted, CreatedAt DESC);
GO

-- 2. Arama motoru performansı için Title indeksi
CREATE INDEX IX_TodoItems_Title 
ON dbo.TodoItems (Title);
GO