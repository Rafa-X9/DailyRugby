using DailyRugby.Application.Interfaces;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class CurrentRoundAutocomplete : AutocompleteHandler
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

        return AutocompletionResult.FromSuccess(roundResult.Item
            .OrderBy(temp => temp.Round)
            .Select(temp => new AutocompleteResult(
                $"{temp.TeamA.Team.Country} vs {temp.TeamB.Team.Country}, round {temp.Round}",
                temp.Id)));
    }
}