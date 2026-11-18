using HR.Data.Entities;
using HR.Services.Assessments;
using HR.Services.Data;

namespace HR.Tests;

public class OverviewFactsTests
{
    [Fact]
    public void MissingDates_ShowADash_AndCountsWhatExists()
    {
        var today = new DateTime(2026, 11, 17);
        var employee = new Employee
        {
            BirthDate = today.AddYears(-30).AddMonths(-2),
            Supplement = new EmployeeSupplement { StartDate = today.AddYears(-2).AddMonths(-1) },
            Courses = new List<Course>
            {
                new() { Title = "ایمنی", CertificateFileId = Guid.NewGuid() },
                new() { Title = "بدون مدرک" }
            },
            Softwares = new List<EmployeeSoftware> { new() { Software = new Software { Title = "Excel" } } },
            Languages = new List<EmployeeLanguage> { new() { Language = new Language { Title = "انگلیسی" } } },
            Leaves = new List<LeaveRecord>
            {
                new() { StartDate = today, EndDate = today.AddDays(2), IsCancelled = false }
            }
        };

        var facts = OverviewFacts.Build(employee, new[]
        {
            new AttemptSummary { AssessmentCode = "self-sliders", SubmittedAt = today, Title = "خودارزیابی" },
            new AttemptSummary { AssessmentCode = "manager-eval", SubmittedAt = today.AddDays(-10), Title = "ارزیابی مدیر" }
        }, today);

        Assert.Equal("2 سال و 1 ماه", facts.Single(item => item.Title == "سابقه کار").Description);
        Assert.Contains("سال", facts.Single(item => item.Title == "سن").Description);
        Assert.Equal("2 دوره، 1 گواهی", facts.Single(item => item.Title == "دوره‌ها").Description);
        Assert.Equal("Excel", facts.Single(item => item.Title == "مهارت نرم‌افزاری").Description);
        Assert.Equal("انگلیسی", facts.Single(item => item.Title == "زبان‌ها").Description);
        Assert.Equal("1 مورد", facts.Single(item => item.Title == "مرخصی امسال").Description);
        Assert.Equal(OverviewFacts.PersianDate(today.AddDays(-10)), facts.Single(item => item.Title == "آخرین آزمون").Description);
        Assert.DoesNotContain(facts, item => item.Description.Contains("خودارزیابی"));
    }

    [Fact]
    public void EmptyEmployee_UsesDashes()
    {
        var facts = OverviewFacts.Build(null, Array.Empty<AttemptSummary>(), DateTime.Today);
        Assert.All(facts, item => Assert.Equal("—", item.Description));
    }
}
