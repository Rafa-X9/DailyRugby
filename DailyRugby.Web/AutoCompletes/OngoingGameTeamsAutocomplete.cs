using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class OngoingGameTeamsAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var gameService = services.GetRequiredService<IGameCrudService>();
        var roundResult = await gameService.GetCurrentRoundAsync();
        if (!roundResult.IsSuccessful)
        {
            return AutocompletionResult.FromSuccess();
        }

        var ongoingGame = roundResult
            .Item
            .FirstOrDefault(temp => temp.CurrentState == GameState.Started);

        if (ongoingGame is null)
        {
            return AutocompletionResult.FromSuccess();
        }

        List<AutocompleteResult> results =
        [
            new AutocompleteResult(ongoingGame.TeamA.Team.Country, nameof(Teams.TeamA)),
            new AutocompleteResult(ongoingGame.TeamB.Team.Country, nameof(Teams.TeamB))
        ];

        return AutocompletionResult.FromSuccess(results);
    }
}