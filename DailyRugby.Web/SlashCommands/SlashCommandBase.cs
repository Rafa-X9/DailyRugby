using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class SlashCommandBase
    : InteractionModuleBase<SocketInteractionContext>
{
    protected override Task<IUserMessage> FollowupAsync(string? text = null,
        Embed[]? embeds = null,
        bool isTTS = false,
        bool ephemeral = false,
        AllowedMentions? allowedMentions = null,
        RequestOptions? options = null,
        MessageComponent? components = null,
        Embed? embed = null,
        PollProperties? poll = null,
        MessageFlags flags = MessageFlags.None)
    {
        if (text is null or { Length: <= 2000 })
        {
            return base.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed, poll, flags);
        }

        using var memory = new MemoryStream(Encoding.UTF8.GetBytes(text));

        return FollowupWithFileAsync(memory,
            "response.txt",
            "The response would be too long, so here's a file instead.",
            embeds,
            isTTS,
            ephemeral,
            allowedMentions,
            components,
            embed,
            options,
            poll,
            flags);
    }

    protected override Task RespondAsync(string? text = null,
        Embed[]? embeds = null,
        bool isTTS = false,
        bool ephemeral = false,
        AllowedMentions? allowedMentions = null,
        RequestOptions? options = null,
        MessageComponent? components = null,
        Embed? embed = null,
        PollProperties? poll = null,
        MessageFlags flags = MessageFlags.None)
    {
        if (text is null or { Length: <= 2000 })
        {
            return base.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed, poll, flags);
        }

        using var memory = new MemoryStream(Encoding.UTF8.GetBytes(text));

        return RespondWithFileAsync(memory,
            "response.txt",
            "The response would be too long, so here's a file instead.",
            embeds,
            isTTS,
            ephemeral,
            allowedMentions,
            components,
            embed,
            options,
            poll,
            flags);
    }

    protected bool CheckRolePermission(IConfiguration configuration)
    {
        string? adminRole = configuration["DailyRugby:AdminRole"];

        if (adminRole is null)
        {
            throw new Exception("Admin role configuration is required");
        }

        var user = Context.User as SocketGuildUser;

        return user?.Roles.Any(role => role.Name == adminRole) ?? false;
    }

    protected string GetUnauthorizedMessage(IConfiguration configuration)
    {
        string? message = configuration["DailyRugby:UnauthorizedMessage"];

        if (message is null)
        {
            throw new Exception("Unauthorized message configuration is required.");
        }

        return message;
    }
}