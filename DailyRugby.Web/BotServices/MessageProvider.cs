namespace DailyRugby.Web.BotServices;

public class MessageProvider
{
    private readonly List<TryAttemptMessage> _tryAttemptMessages;
    private readonly List<DropGoalMessage> _dropGoalMessages;
    private readonly List<PenaltyMessage> _penaltyMessages;
    private readonly List<string> _randomEvents;
    private readonly List<string> _conversionSuccessMessages;
    private readonly List<string> _conversionFailureMessages;
    private readonly List<InjuryRiskMessage> _injuryRiskMessages;
    private readonly List<string> _yellowCardMessages;
    private readonly List<string> _redCardMessages;

    public MessageProvider(IConfiguration configuration)
    {
        _tryAttemptMessages = configuration
            .GetSection("try attempt")
            .Get<List<TryAttemptMessage>>()!;

        _dropGoalMessages = configuration
            .GetSection("drop goal attempt")
            .Get<List<DropGoalMessage>>()!;

        _penaltyMessages = configuration
            .GetSection("penalty attempt")
            .Get<List<PenaltyMessage>>()!;

        _randomEvents = configuration
            .GetSection("random event")
            .Get<List<string>>()!;

        _conversionSuccessMessages = configuration
            .GetSection("conversion success")
            .Get<List<string>>()!;

        _conversionFailureMessages = configuration
            .GetSection("conversion failure")
            .Get<List<string>>()!;

        _injuryRiskMessages = configuration
            .GetSection("injury risk")
            .Get<List<InjuryRiskMessage>>()!;

        _yellowCardMessages = configuration
            .GetSection("yellow card")
            .Get<List<string>>()!;

        _redCardMessages = configuration
            .GetSection("red card")
            .Get<List<string>>()!;
    }
}

public sealed record TryAttemptMessage(string Description,
    string Success,
    string Failure,
    int MinSeconds,
    int MaxSeconds);

public sealed record DropGoalMessage(string Description,
    string Success,
    string Failure,
    int MinSeconds,
    int MaxSeconds);

public sealed record PenaltyMessage(string Description,
    string Success,
    string Failure,
    int MinSeconds,
    int MaxSeconds);

public sealed record InjuryRiskMessage(string Description,
    string DescriptionRecovered,
    string DescriptionNotRecovered);