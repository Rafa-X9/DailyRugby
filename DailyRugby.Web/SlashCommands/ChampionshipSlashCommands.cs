using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using Discord.Interactions;

namespace DailyRugby.Web.SlashCommands;

public class ChampionshipSlashCommands(IChampionshipCrudService champService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("addchampionship", "Creates a championship")]
    public async Task AddChampionship(
        [Summary("name", "The name of the championship")] string name)
    {
        await DeferAsync();

        ChampionshipAddRequest request = new(name);

        var result = await champService.AddAsync(request);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }
        await FollowupAsync($"Success, the championship was created " +
            $"with the Id {result.Item.Id}");
    }
}