using HR.Services.Assessments;

namespace HR.Tests;

public class PersonalityAssessmentTests
{
    [Fact]
    public void Catalog_HasFiveScoredInstruments()
    {
        var all = PersonalityAssessmentCatalog.All();
        Assert.Equal(new[] { "big-five-short", "mbti-style", "disc-short", "riasec-short", "eq-short" }, all.Select(item => item.Code).ToArray());
        Assert.All(all, item =>
        {
            Assert.NotEmpty(item.Questions);
            Assert.NotEmpty(item.Dimensions);
            Assert.Contains(item.Questions, question => !string.IsNullOrWhiteSpace(question.Text));
        });
        Assert.Equal(10, all.Single(item => item.Code == "big-five-short").Questions.Count);
    }

    [Fact]
    public void UntakenAttempt_HasNoStatus_AndSubmittedAttempt_IsCompleted()
    {
        Assert.Equal("none", AttemptStatus.Of(null));
        Assert.Equal("none", AttemptStatus.Of(new AttemptSummary()));
        Assert.Equal("in-progress", AttemptStatus.Of(new AttemptSummary { Status = "InProgress" }));
        Assert.Equal("completed", AttemptStatus.Of(new AttemptSummary
        {
            Status = "Completed",
            SubmittedAt = new DateTime(2026, 11, 1),
            TotalScore = 80,
            LevelLabel = "بالا"
        }));
    }
}
