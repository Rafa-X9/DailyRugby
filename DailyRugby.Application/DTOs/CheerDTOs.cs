namespace DailyRugby.Application.DTOs;

public sealed record CheerAddRequest(ulong UserId,
    bool ForTeamA,
    string? Yell);

public sealed record CheerResponse(ulong UserId,
    bool ForTeamA,
    int CheersLeft,
    string? Yell);