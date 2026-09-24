using Discord.Interactions;
using System.Diagnostics;
using System.Globalization;

namespace DailyRugby.Web.SlashCommands;

public class MiscellaneousSlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("see-ram-usage", "See how much RAM I am using")]
    public async Task SeeRamUsage(
        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        long bytes = Process.GetCurrentProcess().PrivateMemorySize64;
        double megaBytes = bytes / 1_000_000.0;
        await RespondAsync($"I am using {megaBytes.ToString("F2", CultureInfo.InvariantCulture)} " +
            $"megabytes of RAM", ephemeral: @private);
    }
}