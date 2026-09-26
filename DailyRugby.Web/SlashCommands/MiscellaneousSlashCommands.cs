using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Diagnostics;
using System.Globalization;

namespace DailyRugby.Web.SlashCommands;

public class MiscellaneousSlashCommands(MessageSender messageSender, IConfiguration configuration)
    : SlashCommandBase
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

    [SlashCommand("say", "Make me say something in the configured DailyRugby channel")]
    public async Task Say(
        [Summary("Message", "The message to send to the channel.")]
        string message = "Test message")
    {
        if (!this.CheckRolePermission(configuration))
        {
            await RespondAsync(this.GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }

        await messageSender.SendMessageAsync(message);
        await RespondAsync("Done", ephemeral: true);
    }
}