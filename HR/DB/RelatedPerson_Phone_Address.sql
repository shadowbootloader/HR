-- منسوخ: ستون‌های PhoneNumber و Address در LastPersonnel.sql (بلوک UPGRADE EXISTING) هستند.
USE [Personnel];
GO

IF COL_LENGTH('dbo.RelatedPerson', 'PhoneNumber') IS NULL
    ALTER TABLE dbo.RelatedPerson ADD PhoneNumber NVARCHAR(11) NULL;
GO

IF COL_LENGTH('dbo.RelatedPerson', 'Address') IS NULL
    ALTER TABLE dbo.RelatedPerson ADD Address NVARCHAR(300) NULL;
GO
