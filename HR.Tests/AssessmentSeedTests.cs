using HR.Services.Assessments;
using HR.Services.Data;

namespace HR.Tests;

public class AssessmentSeedTests
{
    [Fact]
    public void ManagerSeed_KeepsTheExistingQuestions_AndNothingElse()
    {
        var seed = ManagerAssessmentSeed.Create();

        Assert.Equal("manager-eval", seed.Code);
        Assert.Equal("ارزیابی مدیر", seed.Title);
        Assert.Equal(
            new[]
            {
                "در تصمیم‌گیری‌های تیمی مشارکت فعال دارم",
                "سبک مدیریتی غالب شما کدام است؟",
                "توانایی اولویت‌بندی کارهای تیم را چقدر ارزیابی می‌کنید؟",
                "بازخورد سازنده را شفاف و بدون تنش منتقل می‌کنم",
                "کیفیت ارتباط نوشتاری تیم را چقدر ارزیابی می‌کنید؟",
                "یک موقعیت اخیر را شرح دهید که بازخورد سازنده دادید"
            },
            seed.Questions.Select(question => question.Text).ToArray());

        var choice = seed.Questions.Single(question => question.Code == "q2");
        Assert.Equal(new[] { "دستوری", "مشارکتی", "تفویضی" }, choice.Options.Select(option => option.Text).ToArray());
        Assert.False(seed.Questions.Single(question => question.Code == "q6").CountsTowardTotal);
    }

    [Fact]
    public void RoundTrip_PreservesQuestionCodesAndScores()
    {
        var entity = AssessmentService.ToEntity(ManagerAssessmentSeed.Create(), "آزمون");
        var record = AssessmentService.ToRecord(entity);

        Assert.Equal("dimensionWeighted", record.ScoringMode);
        Assert.Equal(2, record.Dimensions.Count);
        Assert.Equal(1.2, record.Questions.Single(question => question.Code == "q3").Weight);
        Assert.Equal(3, record.Questions.Single(question => question.Code == "q2").Options.Single(option => option.Code == "b").Score);
    }

    [Fact]
    public void SelfAssessment_ReusesTheSliderLabels_AndAveragesThem()
    {
        var definition = SelfAssessmentCatalog.Create();
        Assert.Equal(ProfileInsights.Dimensions.Select(item => item.Label).ToArray(), definition.Questions.Select(item => item.Text).ToArray());

        var attempt = SelfAssessmentCatalog.ToAttempt(Guid.NewGuid(), new SelfAssessmentScores { Discipline = 80, Speed = 40 }, DateTime.UtcNow);
        Assert.Equal(10, attempt.Answers.Count);
        Assert.Equal(80, attempt.Answers[0].NumericValue);
        Assert.Equal(12, attempt.TotalScore);
    }
}
