/*
    SeedExemptions.sql — نوع و علت معافیت نظام‌وظیفه (جدول‌های ExemptionType و ExemptionReason)
    ---------------------------------------------------------------------------------------
    * پیش‌نیاز: LastPersonnel.sql اجرا شده باشد. چند بار اجرا شدنی است و ردیف‌های موجود را تغییر نمی‌دهد.
    * شناسه‌های ۱ تا ۴ علت و ۱ تا ۳ نوع همان‌هایی‌اند که LastPersonnel.sql می‌گذارد؛ بقیه به‌صورت افزوده می‌شوند.
    * عنوان «علت» باید در کل جدول یکتا باشد، چون برنامه علت را فقط با عنوان پیدا می‌کند.
    * قید پایگاه: Employee.(ExemptionReasonId, ExemptionTypeId) باید جفت معتبری از ExemptionReason باشد؛ یعنی علت باید
      متعلق به همان نوع انتخاب‌شده باشد. این فقط وقتی لازم است که MilitaryStatusId = 1 (معاف) باشد.
    * فهرست بر اساس دسته‌بندی‌های رایج نوشته شده؛ ضوابط قانونی دقیق را با سازمان وظیفه عمومی تطبیق دهید.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
USE [Personnel];
GO

INSERT INTO dbo.ExemptionType (ExemptionTypeId, Title)
SELECT v.Id, v.Title
FROM (VALUES
        (1, N'کفالت'),
        (2, N'پزشکی'),
        (3, N'تحصیلی'),
        (4, N'ایثارگری'),
        (5, N'سنی'),
        (6, N'سایر')
) AS v (Id, Title)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ExemptionType x WHERE x.ExemptionTypeId = v.Id OR x.Title = v.Title);
GO

INSERT INTO dbo.ExemptionReason (ExemptionReasonId, ExemptionTypeId, Title)
SELECT v.Id, v.TypeId, v.Title
FROM (VALUES
        (1, 1, N'کفالت پدر'),
        (2, 1, N'کفالت مادر'),
        (3, 2, N'بیماری'),
        (4, 3, N'اشتغال به تحصیل'),
        (5, 1, N'کفالت خواهر یا برادر'),
        (6, 2, N'نقص عضو'),
        (7, 2, N'بیماری روانی'),
        (8, 4, N'فرزند شهید'),
        (9, 4, N'برادر شهید'),
        (10, 4, N'جانباز'),
        (11, 4, N'آزاده'),
        (12, 3, N'تحصیل در حوزه علمیه'),
        (13, 5, N'سن بالاتر از حد مقرر'),
        (14, 6, N'سایر موارد')
) AS v (Id, TypeId, Title)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ExemptionReason x WHERE x.ExemptionReasonId = v.Id OR x.Title = v.Title)
  AND EXISTS (SELECT 1 FROM dbo.ExemptionType t WHERE t.ExemptionTypeId = v.TypeId);
GO

-- بررسی نهایی: هر نوع با علت‌هایش
SELECT t.ExemptionTypeId AS [شناسه نوع], t.Title AS [نوع معافیت], r.ExemptionReasonId AS [شناسه علت], r.Title AS [علت]
FROM dbo.ExemptionType t
LEFT JOIN dbo.ExemptionReason r ON r.ExemptionTypeId = t.ExemptionTypeId
ORDER BY t.ExemptionTypeId, r.ExemptionReasonId;
GO
