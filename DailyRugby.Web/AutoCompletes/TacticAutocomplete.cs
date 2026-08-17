using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class TacticAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        List<AutocompleteResult> tactics =
            [new(Tactics.General.ToString(), Tactics.General.ToString()),
            new(Tactics.Insight.ToString(), Tactics.Insight.ToString()),
            new(Tactics.Physique.ToString(), Tactics.Physique.ToString()),
            new(Tactics.Technique.ToString(), Tactics.Technique.ToString())];
        
        return AutocompletionResult.FromSuccess(tactics);
    }
}