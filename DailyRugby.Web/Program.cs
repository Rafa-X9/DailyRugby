using DailyRugby.Application.CRUD;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Web.SlashCommands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DailyRugby.Web;

class Program
{
    public static readonly DiscordSocketClient DiscordClient = new(new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged
            | GatewayIntents.GuildMembers
            | GatewayIntents.MessageContent
    });

    public const ulong ChannelId = 1535691855969132707;

    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        builder.Services.AddSingleton(connection);

        builder.Services.AddScoped<IChampionshipCrudService, ChampionshipCrudService>();
        builder.Services.AddTransient<ITeamValidatorFactory, TeamValidatorFactory>();
        builder.Services.AddScoped<ITeamCrudService, TeamCrudService>();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connection);
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.MapGet("/", () =>
        {
            return Process.GetCurrentProcess().PrivateMemorySize64;
        });

        string? token = Environment.GetEnvironmentVariable("Bot_Inutil_Token", EnvironmentVariableTarget.User);
        if (token is null)
        {
            throw new Exception("Unable to get discord token");
        }

        DiscordClient.Log += message =>
        {
            app.Logger.LogInformation($"[{message.Severity}] {message.Source}: {message.Message}");

            return Task.CompletedTask;
        };

        var interactions = new InteractionService(DiscordClient.Rest,
            new InteractionServiceConfig
            {
                DefaultRunMode = RunMode.Sync,
                AutoServiceScopes = true
            });

        interactions.Log += message =>
        {
            app.Logger.LogInformation(
                "[Interactions] {Severity}: {Message}",
                message.Severity,
                message.Message);

            if (message.Exception is not null)
            {
                app.Logger.LogError(message.Exception, "[Interactions] Exception");
            }
            return Task.CompletedTask;
        };

        DiscordClient.InteractionCreated += async interaction =>
        {
            var context = new SocketInteractionContext(DiscordClient, interaction);
            var result = await interactions.ExecuteCommandAsync(context, app.Services);
            if (!result.IsSuccess) app.Logger.LogInformation(
                "Execution result: Success={Success}, Error={Error}, Reason={Reason}",
                result.IsSuccess,
                result.Error,
                result.ErrorReason);
        };

        using (var scope = app.Services.CreateScope())
        {
            await interactions.AddModulesAsync(
                typeof(ChampionshipSlashCommands).Assembly,
                scope.ServiceProvider);
        }

        DiscordClient.Ready += async () =>
        {
            await interactions.RegisterCommandsToGuildAsync(1535691855302369370);
        };

        await DiscordClient.LoginAsync(TokenType.Bot, token);
        await DiscordClient.StartAsync();
        await DiscordClient.SetStatusAsync(UserStatus.Online);

        app.Run();
    }
}