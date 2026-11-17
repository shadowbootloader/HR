using HR.Services.Assessments;
using HR.Services.Data;

namespace HR.Tests;

public class ProfileInsightsTests
{
    [Fact]
    public void WithoutARecord_EveryChartIsEmpty()
    {
        var snapshot = ProfileInsights.Build(null, Array.Empty<AttemptSummary>(), Array.Empty<SkillBar>(), Array.Empty<CourseCertificate>(), new DateTime(2026, 11, 17));

        Assert.False(snapshot.HasSelfAssessment);
        Assert.Empty(snapshot.Competencies);
        Assert.Empty(snapshot.Strengths);
        Assert.Empty(snapshot.Improvements);
        Assert.Empty(snapshot.Performance);
        Assert.Empty(snapshot.Achievements);
        Assert.Empty(snapshot.Certificates);
    }

    [Fact]
    public void Sliders_SplitStrengthsAndImprovements_AndSkillsUseTheSeededScale()
    {
        var snapshot = ProfileInsights.Build(
            new SelfAssessmentScores { Discipline = 90, Speed = 30, Effort = 50 },
            new[]
            {
                new AttemptSummary { Title = "ارزیابی مدیر", TotalScore = 80, SubmittedAt = new DateTime(2026, 11, 1), LevelLabel = "قوی" }
            },
            new[]
            {
                new SkillBar { Title = "اکسل", LevelTitle = "پیشرفته" },
                new SkillBar { Title = "ناشناخته", LevelTitle = "استاد" }
            },
            new[]
            {
                new CourseCertificate { Title = "ایمنی", Institution = "مرکز آموزش", HasCertificate = true },
                new CourseCertificate { Title = "بدون مدرک", HasCertificate = false }
            },
            new DateTime(2026, 11, 17));

        Assert.Equal(10, snapshot.Competencies.Count);
        Assert.Contains("نظم", snapshot.Strengths);
        Assert.Contains("سرعت انجام امور", snapshot.Improvements);
        Assert.DoesNotContain(snapshot.Strengths, item => item == "تلاش و پشتکار");
        Assert.Equal(75, snapshot.Skills.Single().Count);
        Assert.Equal("ایمنی", snapshot.Certificates.Single().Title);
        Assert.Equal(80, snapshot.Performance.Single(item => item.Count > 0).Count);
        Assert.Equal("ارزیابی مدیر", snapshot.Achievements.Single().Title);
    }
}
