using DailyRugby.Application.Interfaces;
using Discord;
using Discord.Interactions;

namespace DailyRugby.Web.AutoCompletes;

public class TeamAutoComplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? input = autocompleteInteraction.Data.Current.Value.ToString();
        var teamService = services.GetRequiredService<ITeamCrudService>();

        IEnumerable<AutocompleteResult> list;

        if (string.IsNullOrWhiteSpace(input))
        {
            list = (await teamService.GetAllAsync())
                .OrderByDescending(temp => temp.Id)
                .Select(temp => new AutocompleteResult(
                    $"{temp.Country} - {temp.PlayerUsername}",
                    temp.Id.ToString()))
                .Take(25);
            return AutocompletionResult.FromSuccess(list);
        }

        list = (await teamService.GetAllAsync())
                .Where(temp => $"{temp.Country} - {temp.PlayerUsername}"
                    .Contains(input, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(temp => temp.Id)
                .Select(temp => new AutocompleteResult(
                    $"{temp.Country} by {temp.PlayerUsername}",
                    temp.Id.ToString()))
                .Take(25);

        return AutocompletionResult.FromSuccess(list);
    }
}