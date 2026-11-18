using HR.Services.Data;

namespace HR.Tests;

public class ProfileCompletenessTests
{
    [Fact]
    public void EmptyProfile_ScoresZero_AndListsEveryGap()
    {
        var report = ProfileCompleteness.Score(new ProfileSignals());
        Assert.Equal(0, report.Percent);
        Assert.Equal(100, report.RemainingPercent);
        Assert.Equal(0, report.Score);
        Assert.Contains("عکس", report.Missing);
        Assert.Contains("رزومه", report.Missing);
        Assert.Contains("مدیر مستقیم", report.Missing);
    }

    [Fact]
    public void FullProfile_ScoresAllPoints()
    {
        var report = ProfileCompleteness.Score(new ProfileSignals
        {
            Identity = true,
            Phone = true,
            PostalCode = true,
            Photo = true,
            Department = true,
            Position = true,
            DirectManager = true,
            Education = true,
            Course = true,
            Language = true,
            Software = true,
            Resume = true,
            NationalCard = true,
            SelfAssessment = true,
            EmergencyContact = true
        });
        Assert.Equal(100, report.Percent);
        Assert.Equal(0, report.RemainingPercent);
        Assert.Equal(report.MaxScore, report.Score);
        Assert.Empty(report.Missing);
    }
}
