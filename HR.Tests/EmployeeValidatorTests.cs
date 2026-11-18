using HR.Data.Entities;
using HR.Services.Data;

namespace HR.Tests;

public class EmployeeValidatorTests
{
    private static readonly DateTime Today = new(2026, 11, 17);

    private static Employee Valid() => new()
    {
        FirstName = "نگار",
        LastName = "کریمی",
        FatherName = "رضا",
        NationalCode = "0499370899",
        BirthCertificateNumber = "123",
        BirthDate = new DateTime(1990, 1, 2),
        IssueDate = new DateTime(1990, 1, 3),
        Email = "negar@example.com",
        InsuranceNumber = "1234567890",
        Supplement = new EmployeeSupplement
        {
            PositionTitle = "کارشناس",
            PersonnelCode = "P100",
            StartDate = new DateTime(2020, 3, 21),
            Salary = 0
        }
    };

    [Fact]
    public void ValidEmployee_HasNoProblem()
        => Assert.Null(EmployeeValidator.Validate(Valid(), Today));

    [Fact]
    public void EmployeeWithoutSupplement_IsStillValid()
    {
        var employee = Valid();
        employee.Supplement = null;
        Assert.Null(EmployeeValidator.Validate(employee, Today));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankEmail_IsRejected(string email)
    {
        var employee = Valid();
        employee.Email = email;
        Assert.Equal("ایمیل الزامی است.", EmployeeValidator.Validate(employee, Today));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("a@")]
    [InlineData("Negar <negar@example.com>")]
    public void MalformedEmail_IsRejected(string email)
    {
        var employee = Valid();
        employee.Email = email;
        Assert.Equal("قالب ایمیل نامعتبر است.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void TooLongName_IsRejected()
    {
        var employee = Valid();
        employee.FirstName = new string('ا', EmployeeValidator.NameMaxLength + 1);
        Assert.Contains("نام", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void TooLongInsuranceNumber_IsRejected()
    {
        var employee = Valid();
        employee.InsuranceNumber = new string('1', EmployeeValidator.InsuranceNumberMaxLength + 1);
        Assert.Contains("شماره بیمه", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void TooLongPersonnelCode_IsRejected()
    {
        var employee = Valid();
        employee.Supplement!.PersonnelCode = new string('9', EmployeeValidator.PersonnelCodeMaxLength + 1);
        Assert.Contains("کد پرسنلی", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void FutureBirthDate_IsRejected()
    {
        var employee = Valid();
        employee.BirthDate = Today.AddDays(1);
        employee.IssueDate = Today.AddDays(2);
        Assert.Equal("تاریخ تولد نمی‌تواند در آینده باشد.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void DefaultBirthDate_IsRejected()
    {
        var employee = Valid();
        employee.BirthDate = default;
        Assert.Equal("تاریخ تولد نامعتبر است.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void IssueDateBeforeBirthDate_IsRejected()
    {
        var employee = Valid();
        employee.IssueDate = employee.BirthDate.AddDays(-1);
        Assert.Equal("تاریخ صدور شناسنامه نمی‌تواند قبل از تاریخ تولد باشد.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void BlankPositionTitle_IsRejected()
    {
        var employee = Valid();
        employee.Supplement!.PositionTitle = " ";
        Assert.Equal("سمت الزامی است.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void NegativeSalary_IsRejected()
    {
        var employee = Valid();
        employee.Supplement!.Salary = -1;
        Assert.Equal("حقوق نمی‌تواند منفی باشد.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void EndDateBeforeStartDate_IsRejected()
    {
        var employee = Valid();
        employee.Supplement!.EndDate = employee.Supplement.StartDate.AddDays(-1);
        Assert.Equal("تاریخ پایان همکاری نمی‌تواند قبل از تاریخ شروع باشد.", EmployeeValidator.Validate(employee, Today));
    }

    [Fact]
    public void EndDateOnStartDate_IsAllowed()
    {
        var employee = Valid();
        employee.Supplement!.EndDate = employee.Supplement.StartDate;
        Assert.Null(EmployeeValidator.Validate(employee, Today));
    }
}
