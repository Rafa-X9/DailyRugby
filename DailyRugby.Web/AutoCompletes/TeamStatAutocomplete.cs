using DailyRugby.Application.DTOs;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class TeamStatAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        List<AutocompleteResult> stats =
            [new(TeamStats.Insight.ToString(), TeamStats.Insight.ToString()),
            new(TeamStats.Physique.ToString(), TeamStats.Physique.ToString()),
            new(TeamStats.Technique.ToString(), TeamStats.Technique.ToString())];
        
        return AutocompletionResult.FromSuccess(stats);
    }
}