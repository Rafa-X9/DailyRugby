using DailyRugby.Application.CRUD;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Simulators;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Web.BotServices;
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

    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string? connection = builder.Configuration.GetConnectionString("Sqlite");
        if (connection is null) throw new Exception();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connection);
        });

        builder.Services.AddScoped<IChampionshipCrudService, ChampionshipCrudService>();
        builder.Services.AddTransient<ITeamValidatorFactory, TeamValidatorFactory>();
        builder.Services.AddScoped<ITeamCrudService, TeamCrudService>();
        builder.Services.AddScoped<IGameCrudService, GameCrudService>();
        builder.Services.AddSingleton<GameSimulatorManager>();
        builder.Services.AddSingleton<IGameSimulatorManager>(
            provider => provider.GetRequiredService<GameSimulatorManager>());
        builder.Services.AddHostedService(provider => provider.GetRequiredService<GameSimulatorManager>());
        builder.Services.AddTransient<IGameTimer, SpedUpTimer>();
        builder.Services.AddSingleton<MessageSender>();

        var app = builder.Build();

        _ = app.Services.GetRequiredService<MessageSender>();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
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

        ulong serverId = ulong.Parse(builder.Configuration["ServerId"] ?? "fail");
        ulong channelId = ulong.Parse(builder.Configuration["ChannelId"] ?? "fail");

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
            await interactions.RegisterCommandsToGuildAsync(serverId);
        };

        await DiscordClient.LoginAsync(TokenType.Bot, token);
        await DiscordClient.StartAsync();
        await DiscordClient.SetStatusAsync(UserStatus.Online);

        app.Run();
    }
}