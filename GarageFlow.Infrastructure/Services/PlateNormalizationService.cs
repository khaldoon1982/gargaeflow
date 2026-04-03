using GarageFlow.Application.Interfaces;

namespace GarageFlow.Infrastructure.Services;

public class PlateNormalizationService : IPlateNormalizationService
{
    public string Normalize(string plateNumber)
    {
        if (string.IsNullOrWhiteSpace(plateNumber))
            return string.Empty;

        return plateNumber
            .Trim()
            .Replace("-", "")
            .Replace(" ", "")
            .ToUpperInvariant();
    }
}
