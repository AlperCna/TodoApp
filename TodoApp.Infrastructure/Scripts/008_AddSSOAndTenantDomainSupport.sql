-- Script: 008_AddSSOAndTenantDomainSupport.sql

USE [TodoAppDB];
GO
-- 1. Kullanıcılar tablosuna SSO alanlarını ekleyelim
ALTER TABLE [Users] ADD 
    ExternalProvider NVARCHAR(50) NULL, -- 'Google' veya 'Microsoft' yazacak
    ExternalId NVARCHAR(255) NULL;      -- Azure/Google'dan dönen benzersiz ID

-- 2. Tenants (Şirketler) tablosuna Domain bilgisini ekleyelim
-- Bu sayede @fsm.edu.tr ile gelenin hangi TenantId'ye gideceğini bileceğiz.
ALTER TABLE [Tenants] ADD 
    Domain NVARCHAR(100) NULL; 
GO

