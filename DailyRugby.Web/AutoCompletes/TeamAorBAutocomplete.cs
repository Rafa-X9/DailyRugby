using DailyRugby.Application.DTOs;
using DailyRugby.Domain;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class TeamAorBAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        List<AutocompleteResult> options =
            [new(Teams.TeamA.ToString(), Teams.TeamA.ToString()),
            new(Teams.TeamB.ToString(), Teams.TeamB.ToString())];

        return AutocompletionResult.FromSuccess(options);
    }
}