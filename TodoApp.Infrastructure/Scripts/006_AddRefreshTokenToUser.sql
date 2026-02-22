-- 006_AddRefreshTokenToUser.sql
-- Users tablosuna Refresh Token desteği ekleniyor

USE [TodoAppDB];
GO

ALTER TABLE dbo.Users
ADD RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME2 NULL;
GO