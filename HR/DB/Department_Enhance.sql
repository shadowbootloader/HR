/*
  منسوخ: همین تغییرات داخل بلوک UPGRADE EXISTING در LastPersonnel.sql است.
  گسترش جدول Department برای پوشش کامل فرم
  روی دیتابیس موجود Personnel اجرا شود.
*/
USE [Personnel];
GO

/* ---- ستون‌های جدید روی Department ---- */
IF COL_LENGTH('dbo.Department', 'IsActive') IS NULL
    ALTER TABLE dbo.Department ADD [IsActive] BIT NOT NULL CONSTRAINT DF_Department_IsActive DEFAULT (1);
GO

IF COL_LENGTH('dbo.Department', 'SecurityLevel') IS NULL
    ALTER TABLE dbo.Department ADD [SecurityLevel] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.Department', 'EstablishedDate') IS NULL
    ALTER TABLE dbo.Department ADD [EstablishedDate] DATE NULL;
GO

IF COL_LENGTH('dbo.Department', 'ApprovedCapacity') IS NULL
    ALTER TABLE dbo.Department ADD [ApprovedCapacity] INT NULL;
GO

IF COL_LENGTH('dbo.Department', 'LogoPath') IS NULL
    ALTER TABLE dbo.Department ADD [LogoPath] NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.Department', 'ManagerEmployeeId') IS NULL
    ALTER TABLE dbo.Department ADD [ManagerEmployeeId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Department', 'DeputyEmployeeId') IS NULL
    ALTER TABLE dbo.Department ADD [DeputyEmployeeId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Department', 'CashCustodianEmployeeId') IS NULL
    ALTER TABLE dbo.Department ADD [CashCustodianEmployeeId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Department', 'CostCenterCode') IS NULL
    ALTER TABLE dbo.Department ADD [CostCenterCode] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.Department', 'AnnualBudget') IS NULL
    ALTER TABLE dbo.Department ADD [AnnualBudget] DECIMAL(18,0) NULL;
GO

IF COL_LENGTH('dbo.Department', 'PettyCashLimit') IS NULL
    ALTER TABLE dbo.Department ADD [PettyCashLimit] DECIMAL(18,0) NULL;
GO

IF COL_LENGTH('dbo.Department', 'Description') IS NULL
    ALTER TABLE dbo.Department ADD [Description] NVARCHAR(500) NULL;
GO

/* یکسان‌سازی نام ستون والد: اگر ParentDepartment بود و ParentDepartmentId نبود */
IF COL_LENGTH('dbo.Department', 'ParentDepartment') IS NOT NULL
   AND COL_LENGTH('dbo.Department', 'ParentDepartmentId') IS NULL
BEGIN
    EXEC sp_rename 'dbo.Department.ParentDepartment', 'ParentDepartmentId', 'COLUMN';
END
GO

/* FK مدیر / جانشین / تنخواه‌دار به Employee (اختیاری) */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Department_Manager')
BEGIN
    ALTER TABLE dbo.Department WITH NOCHECK
    ADD CONSTRAINT FK_Department_Manager FOREIGN KEY (ManagerEmployeeId)
        REFERENCES dbo.Employee(EmployeeId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Department_Deputy')
BEGIN
    ALTER TABLE dbo.Department WITH NOCHECK
    ADD CONSTRAINT FK_Department_Deputy FOREIGN KEY (DeputyEmployeeId)
        REFERENCES dbo.Employee(EmployeeId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Department_CashCustodian')
BEGIN
    ALTER TABLE dbo.Department WITH NOCHECK
    ADD CONSTRAINT FK_Department_CashCustodian FOREIGN KEY (CashCustodianEmployeeId)
        REFERENCES dbo.Employee(EmployeeId);
END
GO

/* ---- جدول دفاتر دپارتمان ---- */
IF OBJECT_ID(N'dbo.DepartmentOffice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DepartmentOffice
    (
        OfficeId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DepartmentOffice_Id DEFAULT (NEWSEQUENTIALID()),
        DepartmentId SMALLINT         NOT NULL,
        Name         NVARCHAR(100)    NOT NULL,
        City         NVARCHAR(100)    NULL,
        Address      NVARCHAR(500)    NULL,
        CONSTRAINT PK_DepartmentOffice PRIMARY KEY CLUSTERED (OfficeId),
        CONSTRAINT FK_DepartmentOffice_Department FOREIGN KEY (DepartmentId)
            REFERENCES dbo.Department(DepartmentId) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_DepartmentOffice_DepartmentId
        ON dbo.DepartmentOffice(DepartmentId);
END
GO

/* داده اولیه اگر جدول خالی است */
IF NOT EXISTS (SELECT 1 FROM dbo.Department)
BEGIN
    INSERT INTO dbo.Department (Title, ParentDepartmentId, IsActive)
    VALUES (N'منابع انسانی', NULL, 1),
           (N'فناوری اطلاعات', NULL, 1),
           (N'مالی و حسابداری', NULL, 1);
END
GO
