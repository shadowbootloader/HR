using HR.Services.Data;

namespace HR.Tests;

public class SecurityLevelTests
{
    [Fact]
    public void Ranks_MatchTheOfficialList()
    {
        Assert.Equal(
            new[] { "عادی", "محرمانه", "خیلی محرمانه", "فوق محرمانه", "سری", "بسیار سری" },
            SecurityLevels.All);
    }
}
