namespace DailyRugby.Application.DTOs;

public record CheerAddRequest(ulong UserId,
    bool ForTeamA,
    string? Yell);