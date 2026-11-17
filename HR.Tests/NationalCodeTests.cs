using HR.Services;

namespace HR.Tests;

public class NationalCodeTests
{
    [Theory]
    [InlineData("1234567891", true)]
    [InlineData("1234567890", false)]
    [InlineData("0000000000", false)]
    [InlineData("123456789", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CheckDigit_MatchesTheIranianRule(string? code, bool expected)
        => Assert.Equal(expected, NationalCode.IsValid(code));
}
