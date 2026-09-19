using Discord.Interactions;
using Discord.WebSocket;

namespace DailyRugby.Web.BotServices;

public static class InteractionModuleExtensions
{
    extension(InteractionModuleBase<SocketInteractionContext> module)
    {
        public string UnauthorizedMessage => "Hey, you aren't Aart ˙◠˙";
    }

    public static bool CheckRolePermission(
        this InteractionModuleBase<SocketInteractionContext> module)
    {
        var user = module.Context.User as SocketGuildUser;

        return user?.Roles.Any(role => role.Name == "Botbouwer") ?? false;
    }
}