using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class SeasonAutoComplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        List<AutocompleteResult> seasons =
            [new(Seasons.Season1.ToString(), Seasons.Season1.ToString()),
            new(Seasons.Season3.ToString(), Seasons.Season3.ToString()),
            new(Seasons.Season4.ToString(), Seasons.Season4.ToString())];
        return AutocompletionResult.FromSuccess(seasons);
    }
}