namespace DailyRugby.Application.DTOs;

public record ScheduleResponse(DateTime Date,
    string TeamA,
    string TeamB);