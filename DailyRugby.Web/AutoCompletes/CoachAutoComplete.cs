using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class CoachAutoComplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        List<AutocompleteResult> coaches =
            [new(Coaches.General.ToString(), Coaches.General.ToString()),
            new(Coaches.Insight.ToString(), Coaches.Insight.ToString()),
            new(Coaches.Physique.ToString(), Coaches.Physique.ToString()),
            new(Coaches.Technique.ToString(), Coaches.Technique.ToString())];

        return AutocompletionResult.FromSuccess(coaches);
    }
}