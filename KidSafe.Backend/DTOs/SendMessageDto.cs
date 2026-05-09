namespace KidSafe.Backend.DTOs;

public record SendMessageDto(int ReceiverId, string Message);
public record ClassMessageDto(string Content);
public record AnalyzeRequestDto(string Message);
public record AnalyzeResponseDto(string Label, double Score);
public record MessageResultDto(string Status, string? MaskedMessage, string Label, double Score);
