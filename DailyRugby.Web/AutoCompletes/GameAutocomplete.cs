using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class GameAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var gameService = services.GetRequiredService<IGameCrudService>();
        var games = (await gameService.GetAllAsync())
            .Where(temp => temp.CurrentState == GameState.NotScheduled)
            .OrderBy(temp => temp.Id)
            .Select(temp => new AutocompleteResult(
                $"{temp.TeamA.Team.Country} vs {temp.TeamB.Team.Country}, round {temp.Round}",
                temp.Id.ToString()))
            .Take(25);

        return AutocompletionResult.FromSuccess(games);
    }
}