using GarageFlow.Infrastructure.Services;
using Xunit;

namespace GarageFlow.Tests.Services;

public class PlateNormalizationTests
{
    private readonly PlateNormalizationService _service = new();

    [Theory]
    [InlineData("12-ab-34", "12AB34")]
    [InlineData(" 99-KL-7 ", "99KL7")]
    [InlineData("AB-123-CD", "AB123CD")]
    [InlineData("ab 123 cd", "AB123CD")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Normalize_ShouldReturnExpectedResult(string? input, string expected)
    {
        var result = _service.Normalize(input!);
        Assert.Equal(expected, result);
    }
}
