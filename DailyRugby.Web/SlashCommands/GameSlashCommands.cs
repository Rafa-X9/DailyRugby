using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DailyRugby.Web.SlashCommands;

public class GameSlashCommands(IGameCrudService gameService,
    IGameSimulatorManager simulator,
    IGameOddsCalculator oddsCalculator)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("see-games", "Shows all games from a championship")]
    public async Task SeeGames(
        [Summary("championship", "The championship to show the games from")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var list = (await gameService.GetAllAsync(id))
            .OrderBy(temp => temp.Round)
            .ToList();

        if (list.Count == 0)
        {
            await FollowupAsync("That championship has no games", ephemeral: true);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("All games in the championship:");
        foreach (var game in list)
        {
            sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country} " +
                $"in Round {game.Round}");
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("see-teams-games", "Sees all the games of a specific team")]
    public async Task SeeTeamsGames(
        [Summary("team", "The team to see the games")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var gamesResult = await gameService.GetByTeamIdAsync(id);

        if (!gamesResult.IsSuccessful)
        {
            await FollowupAsync($"{gamesResult.Error}: {gamesResult.Message}", ephemeral: true);
            return;
        }

        var sortedGames = gamesResult.Item.OrderBy(temp => temp.Round).ToList();

        var requestedTeam = gamesResult.Item
            .SelectMany(temp => new List<TeamResponse> { temp.TeamA.Team, temp.TeamB.Team })
            .First(temp => temp.Id == id);

        StringBuilder sb = new();
        sb.AppendLine($"These are all {requestedTeam.Country}'s games:");

        foreach (var game in sortedGames)
        {
            if (game.TeamA.Team.Id == id)
            {
                sb.AppendLine($"- vs {game.TeamB.Team.Country} on round {game.Round}");
            }
            else
            {
                sb.AppendLine($"- vs {game.TeamA.Team.Country} on round {game.Round}");
            }
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("schedule-game", "Schedules a game")]
    public async Task ScheduleGame(
        [Summary("game", "The game to schedule")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,
        int yearUtc,
        int monthUtc,
        int dayUtc,
        int hourUtc,
        int minuteUtc)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        DateTime dateTime = new(yearUtc, monthUtc, dayUtc, hourUtc, minuteUtc, 0);

        var result = await simulator.ScheduleGameAsync(id, dateTime);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync("Scheduled successfully", ephemeral: true);
    }

    [SlashCommand("see-current-round", "Shows all games from the current round")]
    public async Task SeeCurrentRound(
        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        var result = await gameService.GetCurrentRoundAsync();

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"**ROUND {result.Item.First().Round}**");
        foreach (var game in result.Item)
        {
            sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country}");
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("set-tactic", "Sets a team's tactic")]
    public async Task SetTactic(
        [Summary("Game", "The game to set a tactic")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Tactic", "The tactic to set")]
        [Autocomplete(typeof(TacticAutocomplete))]
        string tactic,

        [Summary("Team", "The team that should have the tactic applied to")]
        [Autocomplete(typeof(TeamAorBAutocomplete))]
        string teamAorB)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        bool tacticParsed = Enum.TryParse(tactic, true, out Tactics tacticEnum);
        if (!tacticParsed)
        {
            await FollowupAsync("Invalid tactic", ephemeral: true);
            return;
        }

        bool teamParsed = Enum.TryParse(teamAorB, true, out Teams team);
        if (!teamParsed)
        {
            await FollowupAsync("Invalid team to set tactic to", ephemeral: true);
            return;
        }

        var result = await gameService.SetTacticAsync(id, team, tacticEnum);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"Successfully applied the tactic {result.Item.Tactic} " +
            $"to {result.Item.Team.Country}", ephemeral: true);
    }

    [SlashCommand("set-coach", "Set the coach a team will use for a game")]
    public async Task SetCoach(
        [Summary("Game", "The game to set a tactic")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Coach", "The coach to set")]
        [Autocomplete(typeof(CoachAutoComplete))]
        string coach,

        [Summary("Team", "The team that should have the tactic applied to")]
        [Autocomplete(typeof(TeamAorBAutocomplete))]
        string teamAorB)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        bool coachParsed = Enum.TryParse(coach, true, out Coaches enumCoach);
        if (!coachParsed)
        {
            await FollowupAsync("Invalid coach", ephemeral: true);
            return;
        }

        bool teamParsed = Enum.TryParse(teamAorB, true, out Teams team);
        if (!teamParsed)
        {
            await FollowupAsync("Invalid team to set tactic to", ephemeral: true);
            return;
        }

        var result = await gameService.SetCoachAsync(id, team, enumCoach);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"Successfully applied the coach {result.Item.Coach} " +
            $"to {result.Item.Team.Country}", ephemeral: true);
    }

    [SlashCommand("see-odds", "Shows the odds of a game")]
    public async Task SeeOdds(
        [Summary("Game", "The game to get odds from")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var oddsResult = await oddsCalculator.GetOddsAsync(id, passIfNotExists: true);

        if (!oddsResult.IsSuccessful)
        {
            await FollowupAsync(oddsResult.Message, ephemeral: true);
            _ = oddsCalculator.GetOddsAsync(id, passIfNotExists: false);
            return;
        }

        var gameResult = await gameService.GetByIdAsync(oddsResult.Item.GameId);

        if (!gameResult.IsSuccessful)
        {
            await FollowupAsync("Somehow, getting the odds suceeded but getting the game failed",
                ephemeral: true);
            return;
        }

        decimal teamAWins = ((decimal)oddsResult.Item.TeamAWins / oddsResult.Item.TotalSimulations) * 100;
        decimal teamBWins = ((decimal)oddsResult.Item.TeamBWins / oddsResult.Item.TotalSimulations) * 100;

        CultureInfo c = CultureInfo.InvariantCulture;

        StringBuilder sb = new();
        sb.AppendLine("These were the simulations:");
        sb.AppendLine($"{gameResult.Item.TeamA.Team.Country} wins: {teamAWins.ToString("F0", c)}%");
        sb.AppendLine($"{gameResult.Item.TeamB.Team.Country} wins: {teamBWins.ToString("F0", c)}%");

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("cheer", "Cheer for a team!")]
    public async Task Cheer(
        [Summary("Team", "The team to cheer for")]
        [Autocomplete(typeof(OngoingGameTeamsAutocomplete))]
        string teamAorB,

        [Summary("Yell", "What do you yell for the team?")]
        string? yell = null)
    {
        if (yell is { Length: > 1000 })
        {
            await RespondAsync("That yell is too long!", ephemeral: true);
            return;
        }

        bool teamParsed = Enum.TryParse(teamAorB, true, out Teams team);
        if (!teamParsed)
        {
            await RespondAsync("Invalid team to cheer for", ephemeral: true);
            return;
        }

        var user = Context.User;

        CheerAddRequest request = new(user.Id,
            team == Teams.TeamA,
            yell);

        var result = simulator.AddCheer(request);

        if (!result.IsSuccessful)
        {
            await RespondAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await RespondAsync("Your cheer has been scheduled", ephemeral: true);
    }

    [SlashCommand("see-current-round-json", "Get the current round as JSON")]
    public async Task SeeCurrentRoundJson()
    {
        await DeferAsync(ephemeral: true);

        var result = await gameService.GetCurrentRoundAsync();

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        var games = new List<object>();

        foreach (var game in result.Item)
        {
            games.Add(new
            {
                teamA = game.TeamA.Team.Country,
                teamB = game.TeamB.Team.Country,

                teamAHasMoraleBoost = game.TeamA.HasMoraleBoost,
                teamBHasMoraleBoost = game.TeamB.HasMoraleBoost,

                teamAGetsMoraleBoost = game.TeamA.GetsMoraleBoostIfWins,
                teamBGetsMoraleBoost = game.TeamB.GetsMoraleBoostIfWins
            });
        }

        string json = JsonSerializer.Serialize(games);

        await FollowupAsync(json, ephemeral: true);
    }

    [SlashCommand("see-previous-round-json", "Get the previous round as JSON")]
    public async Task SeePreviousRoundJson()
    {
        await DeferAsync(ephemeral: true);

        var currentRoundResult = await gameService.GetCurrentRoundAsync();

        if (!currentRoundResult.IsSuccessful)
        {
            await FollowupAsync($"{currentRoundResult.Error}: {currentRoundResult.Message}",
                ephemeral: true);
            return;
        }

        if (currentRoundResult.Item.Count == 0)
        {
            await FollowupAsync("There is no previous round.");
            return;
        }

        Guid champId = currentRoundResult.Item[0].ChampId;
        int round = currentRoundResult.Item[0].Round - 1;

        var result = await gameService.GetRoundAsync(champId, round);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        var games = new List<object>();

        foreach (var game in result.Item)
        {
            games.Add(new
            {
                teamA = game.TeamA.Team.Country,
                teamB = game.TeamB.Team.Country,

                teamAScore = game.TeamAScore,
                teamBScore = game.TeamBScore,

                teamATactic = game.TeamA.Tactic.ToString(),
                teamBTactic = game.TeamB.Tactic.ToString(),

                teamAUsedCake = game.TeamA.IsUsingCake,
                teamBUsedCake = game.TeamB.IsUsingCake,

                teamAHadMoraleBoost = game.TeamA.HasMoraleBoost,
                teamBHadMoraleBoost = game.TeamB.HasMoraleBoost,

                teamAGotMoraleBoost = game.TeamA.GetsMoraleBoostIfWins,
                teamBGotMoraleBoost = game.TeamB.GetsMoraleBoostIfWins
            });
        }

        string json = JsonSerializer.Serialize(games);

        await FollowupAsync(json, ephemeral: true);
    }
}