USE [Personnel];
GO

/* وظیفه ۲ — تفکیک RelatedPerson */
IF COL_LENGTH('dbo.RelatedPerson', 'PersonRole') IS NULL
BEGIN
    ALTER TABLE dbo.RelatedPerson
        ADD PersonRole TINYINT NOT NULL
            CONSTRAINT DF_RelatedPerson_PersonRole DEFAULT (0);
END
GO

/* وظیفه ۸ — Ability / WorkInterest / SelfAssessment */
IF OBJECT_ID(N'dbo.Ability', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ability (
        AbilityId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ability PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL);
    CREATE UNIQUE INDEX UX_Ability_Title ON dbo.Ability (Title);
END
GO

IF OBJECT_ID(N'dbo.EmployeeAbility', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeAbility (
        EmployeeAbilityId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmployeeAbility PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_EmployeeAbility_Employee REFERENCES dbo.Employee (EmployeeId),
        AbilityId INT NOT NULL
            CONSTRAINT FK_EmployeeAbility_Ability REFERENCES dbo.Ability (AbilityId),
        SkillLevelId TINYINT NOT NULL
            CONSTRAINT FK_EmployeeAbility_SkillLevel REFERENCES dbo.SkillLevel (SkillLevelId),
        Description NVARCHAR(500) NULL);
END
GO

IF OBJECT_ID(N'dbo.InterestLevel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InterestLevel (
        InterestLevelId TINYINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InterestLevel PRIMARY KEY,
        Title NVARCHAR(50) NOT NULL);
    CREATE UNIQUE INDEX UX_InterestLevel_Title ON dbo.InterestLevel (Title);

    INSERT INTO dbo.InterestLevel (Title) VALUES (N'کم'), (N'متوسط'), (N'زیاد');
END
GO

IF OBJECT_ID(N'dbo.EmployeeWorkInterest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeWorkInterest (
        EmployeeWorkInterestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmployeeWorkInterest PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_EmployeeWorkInterest_Employee REFERENCES dbo.Employee (EmployeeId),
        InterestLevelId TINYINT NOT NULL
            CONSTRAINT FK_EmployeeWorkInterest_InterestLevel REFERENCES dbo.InterestLevel (InterestLevelId),
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL);
END
GO

IF OBJECT_ID(N'dbo.EmployeeSelfAssessment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeSelfAssessment (
        EmployeeSelfAssessmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmployeeSelfAssessment PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_EmployeeSelfAssessment_Employee REFERENCES dbo.Employee (EmployeeId),
        Discipline TINYINT NOT NULL,
        Speed TINYINT NOT NULL,
        Effort TINYINT NOT NULL,
        Negotiation TINYINT NOT NULL,
        Patience TINYINT NOT NULL,
        Creativity TINYINT NOT NULL,
        Punctuality TINYINT NOT NULL,
        Teamwork TINYINT NOT NULL,
        Accuracy TINYINT NOT NULL,
        Planning TINYINT NOT NULL,
        Description NVARCHAR(500) NULL);
END
GO

/* FileTypeهای لازم برای تصاویر */
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'عکس پرسنلی')
    INSERT INTO dbo.FileType (Title) VALUES (N'عکس پرسنلی');
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'کارت ملی')
    INSERT INTO dbo.FileType (Title) VALUES (N'کارت ملی');
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'شناسنامه')
    INSERT INTO dbo.FileType (Title) VALUES (N'شناسنامه');
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'پایان خدمت')
    INSERT INTO dbo.FileType (Title) VALUES (N'پایان خدمت');
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'رزومه')
    INSERT INTO dbo.FileType (Title) VALUES (N'رزومه');
IF NOT EXISTS (SELECT 1 FROM dbo.FileType WHERE Title = N'مدرک تحصیلی')
    INSERT INTO dbo.FileType (Title) VALUES (N'مدرک تحصیلی');
GO
