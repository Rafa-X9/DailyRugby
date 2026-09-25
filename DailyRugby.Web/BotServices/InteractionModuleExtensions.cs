using Discord.Interactions;
using Discord.WebSocket;

namespace DailyRugby.Web.BotServices;

public static class InteractionModuleExtensions
{
    public static bool CheckRolePermission(
        this InteractionModuleBase<SocketInteractionContext> module,
        IConfiguration configuration)
    {
        string? adminRole = configuration["DailyRugby:AdminRole"];

        if (adminRole is null)
        {
            throw new Exception("Admin role configuration is required");
        }

        var user = module.Context.User as SocketGuildUser;

        return user?.Roles.Any(role => role.Name == adminRole) ?? false;
    }

    public static string GetUnauthorizedMessage(
        this InteractionModuleBase<SocketInteractionContext> module,
        IConfiguration configuration)
    {
        string? message = configuration["DailyRugby:UnauthorizedMessage"];

        if (message is null)
        {
            throw new Exception("Unauthorized message configuration is required.");
        }

        return message;
    }
}