using KidSafe.Backend.DTOs;

namespace KidSafe.Backend.Services;

public interface IAIService
{
    Task<AnalyzeResponseDto> AnalyzeAsync(string message);
    string MaskMessage(string message);
}
