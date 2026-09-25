using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Domain.Migrations;
using DailyRugby.Web.AutoCompletes;
using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DailyRugby.Web.SlashCommands;

public class GameSlashCommands(IGameCrudService gameService,
    IGameSimulatorManager simulator,
    IGameOddsCalculator oddsCalculator,
    IJsonGetter jsonGetter,
    IConfiguration configuration)
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

    [SlashCommand("see-game-details", "Shows all details of a game, including tactics and cakes")]
    public async Task SeeGameDetails(
        [Summary("Game", "The game to set a tactic")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId)
    {
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var gameResult = await gameService.GetByIdAsync(id);

        if (!gameResult.IsSuccessful)
        {
            await FollowupAsync($"{gameResult.Error}: {gameResult.Message}", ephemeral: true);
            return;
        }

        var game = gameResult.Item;

        await FollowupAsync($"# {game.TeamA.Team.Country} vs {game.TeamB.Team.Country}\n" +
            $"\n" +
            $"**{game.TeamA.Team.Country}**:\n" +
            $"- {game.TeamA.Coach} coach\n" +
            $"- {game.TeamA.Tactic} tactic\n" +
            $"- {game.TeamA.Cake?.Name ?? "no"} cake\n" +
            $"- {(game.TeamA.HasMoraleBoost ? "has morale boost" : "doesn't have morale boost")}\n" +
            $"\n" +
            $"**{game.TeamB.Team.Country}**:\n" +
            $"- {game.TeamB.Coach} coach\n" +
            $"- {game.TeamB.Tactic} tactic\n" +
            $"- {game.TeamB.Cake?.Name ?? "no"} cake\n" + 
            $"- {(game.TeamB.HasMoraleBoost ? "has morale boost" : "doesn't have morale boost")}",
            
            ephemeral: true);
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
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
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
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
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
        [Summary("Game", "The game to set a coach")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Coach", "The coach to set")]
        [Autocomplete(typeof(CoachAutoComplete))]
        string coach,

        [Summary("Team", "The team that should have the tactic applied to")]
        [Autocomplete(typeof(TeamAorBAutocomplete))]
        string teamAorB)
    {
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
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

    [SlashCommand("set-cake", "Set the cake a team will use in a game")]
    public async Task SetCake(
        [Summary("Game", "The game to set a cake")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Cake", "The flavor of the cake that will be used")]
        string cake,

        [Summary("Team", "Which team will use the cake")]
        [Autocomplete(typeof(TeamAorBAutocomplete))]
        string teamAorB)
    {
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        bool teamParsed = Enum.TryParse(teamAorB, true, out Teams team);
        if (!teamParsed)
        {
            await FollowupAsync("Invalid team to set cake to", ephemeral: true);
            return;
        }

        var result = await gameService.SetCakeAsync(id, team, cake);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"Successfully set the {result.Item.Cake!.Name} cake to " +
            $"{result.Item.Team.Country}'s next game.", ephemeral: true);
    }

    [SlashCommand("set-morale-boost", "Add the morale boost for a team")]
    public async Task SetMoraleBoost(
        [Summary("Game", "The game to set a morale boost")]
        [Autocomplete(typeof(CurrentRoundAutocomplete))]
        string gameId,

        [Summary("Team", "Which team will get the morale boost")]
        [Autocomplete(typeof(TeamAorBAutocomplete))]
        string teamAorB,

        [Summary("MoraleBoost", "Whether the team should have a morale boost added or removed")]
        bool hasMoraleBoost)
    {
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        bool teamParsed = Enum.TryParse(teamAorB, true, out Teams team);
        if (!teamParsed)
        {
            await FollowupAsync("Invalid team to set morale boost to", ephemeral: true);
            return;
        }

        var result = await gameService.SetMoraleBoostAsync(id, team, hasMoraleBoost);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"{result.Item.Team.Country}'s morale boost has been set " +
            $"set to {result.Item.HasMoraleBoost}.");
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

        var dataResult = await jsonGetter.GetCurrentRoundAsync();

        if (!dataResult.IsSuccessful)
        {
            await FollowupAsync($"{dataResult.Error}: {dataResult.Message}", ephemeral: true);
            return;
        }

        string json = JsonSerializer.Serialize(dataResult.Item);

        await FollowupAsync(json, ephemeral: true);
    }

    [SlashCommand("see-previous-round-json", "Get the previous round as JSON")]
    public async Task SeePreviousRoundJson()
    {
        await DeferAsync(ephemeral: true);

        var dataResult = await jsonGetter.GetPreviousRoundAsync();

        if (!dataResult.IsSuccessful)
        {
            await FollowupAsync($"{dataResult.Error}: {dataResult.Message}");
            return;
        }

        string json = JsonSerializer.Serialize(dataResult.Item);

        await FollowupAsync(json, ephemeral: true);
    }

    [SlashCommand("see-team-description", "Describe a team from the ongoing game")]
    public async Task SeeTeamDescription(
        [Summary("Team", "The team to describe")]
        [Autocomplete(typeof(OngoingGameTeamsAutocomplete))]
        string team,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        bool teamParsed = Enum.TryParse(team, true, out Teams enumTeam);

        if (!teamParsed)
        {
            await RespondAsync("Team isn't a valid team", ephemeral: true);
            return;
        }

        var playersResult = simulator.GetPlayersFromGame(enumTeam);

        if (!playersResult.IsSuccessful)
        {
            await RespondAsync("There isn't an ongoing game", ephemeral: true);
            return;
        }

        var playersOnField = playersResult.Item
            .Where(temp => temp.IsOnField)
            .Select(temp => temp.Number.ToString())
            .ToArray();

        var replacementPlayers = playersResult.Item
            .Where(temp => !temp.IsOnField && temp.CanJoinField)
            .Select(temp => temp.Number.ToString())
            .ToArray();

        var outPlayers = playersResult.Item
            .Where(temp => !temp.IsOnField && !temp.CanJoinField)
            .Select(temp => temp.Number.ToString())
            .ToArray();

        StringBuilder sb = new();

        if (playersOnField.Length > 0)
        {
            sb.AppendLine($"The team has {playersOnField.Length} players on the field. They are: " +
                $"{string.Join(", ", playersOnField)}.\n");
        }
        else sb.AppendLine("The team has no players on the field");

        if (replacementPlayers.Length > 0)
        {
            sb.AppendLine($"The team has {replacementPlayers.Length} replacement players available. They " +
                $"are: {string.Join(", ", replacementPlayers)}.\n");
        }
        else sb.AppendLine("The team has no replacement players available.\n");

        if (outPlayers.Length > 0)
        {
            sb.AppendLine($"The team has {outPlayers.Length} players who have left the game. They " +
                $"are: {string.Join(", ", outPlayers)}.");
        }
        else sb.AppendLine("The team has no players who have left the game.");

        await RespondAsync(sb.ToString(), ephemeral: @private);
    }
}