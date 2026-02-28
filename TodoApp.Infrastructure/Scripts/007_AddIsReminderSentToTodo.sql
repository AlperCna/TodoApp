
USE [TodoAppDB];
GO

-- TodoItems tablosuna hatırlatıcı durumunu takip edecek kolon ekleniyor
-- Default 0 (false) olarak başlar
ALTER TABLE TodoItems 
ADD IsReminderSent BIT NOT NULL DEFAULT 0;
GO