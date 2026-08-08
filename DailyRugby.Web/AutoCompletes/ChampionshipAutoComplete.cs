using DailyRugby.Application.Interfaces;
using Discord;
using Discord.Interactions;
using Microsoft.AspNetCore.Mvc;

namespace DailyRugby.Web.AutoCompletes;

public class ChampionshipAutoComplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        string? input = autocompleteInteraction.Data.Current.Value.ToString();
        var champService = services.GetRequiredService<IChampionshipCrudService>();

        IEnumerable<AutocompleteResult> list;

        if (string.IsNullOrWhiteSpace(input))
        {
            list = (await champService.GetAllAsync())
                .Select(temp => new AutocompleteResult(temp.Name, temp.Id.ToString()))
                .Take(25);
            return AutocompletionResult.FromSuccess(list);
        }

        list = (await champService.GetAllAsync())
            .Where(temp => temp.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Select(temp => new AutocompleteResult(temp.Name, temp.Id.ToString()))
            .Take(25);
        return AutocompletionResult.FromSuccess(list);
    }
}