# چک‌لیست تست دود (Smoke Test) — سامانه HR

## پیش‌نیاز
- اتصال به دیتابیس `Personnel` برقرار باشد.
- اسکریپت‌های اسکیما اجرا شده باشند.
- جداول Lookup پایه پر باشند و حداقل یک دپارتمان وجود داشته باشد.

## ۱) ثبت کامل کارمند
1. به «افزودن کارمند» برو.
2. اطلاعات شخصی (شامل معافیت و مذهب)، تلفن، تحصیلات، بانک، بستگان (تلفن و آدرس)، زبان، نرم‌افزار و سایر بخش‌ها را پر کن.
3. ذخیره کن و پیام موفقیت را ببین.

## ۲) باز کردن مجدد
1. از لیست روی ویرایش همان کارمند بزن.
2. همه بخش‌های پرشده باید دوباره در فرم باشند.

## ۳) ویرایش یک فیلد
1. فقط یک فیلد (مثلاً نام) را عوض کن و ذخیره کن.
2. دوباره باز کن؛ بقیه ردیف‌ها نباید از بین رفته باشند.

## ۴) حذف همه حساب‌های بانکی
1. همه حساب‌ها را حذف و ذخیره کن.
2. `EmployeeSupplement.BankAccountId` باید `NULL` باشد.

## ۵) کد ملی تکراری
1. کارمند جدید با همان کد ملی بساز؛ باید رد شود.

## ۶) اعتبارسنجی کد ملی ۱۰ رقمی
1. کد ملی نامعتبر وارد کن؛ ذخیره نباید انجام شود.

## ۷) ریست دیتابیس (فقط توسعه)
```sql
USE Personnel;
DELETE FROM EmployeeAbility;
DELETE FROM EmployeeWorkInterest;
DELETE FROM EmployeeSelfAssessment;
DELETE FROM PersonFile;
DELETE FROM BackgroundRecord;
DELETE FROM HiringRecord;
DELETE FROM SacrificeRecord;
DELETE FROM OtherActivity;
DELETE FROM OrganizationCooperation;
DELETE FROM ForeignCooperation;
DELETE FROM ForeignRelative;
DELETE FROM ForeignTravel;
DELETE FROM Citizenship;
DELETE FROM EmploymentHistory;
DELETE FROM EmployeeSoftware;
DELETE FROM EmployeeLanguage;
DELETE FROM RelatedPerson;
DELETE FROM PhoneNumber;
DELETE FROM BankAccount;
DELETE FROM Course;
DELETE FROM Education;
DELETE FROM EmployeeSupplement;
DELETE FROM Employee;
```
