using Discord.Interactions;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class DatabaseSlashCommands(IConfiguration configuration)
    : SlashCommandBase
{
    [SlashCommand("backup-database", "Makes a backup of the database")]
    public async Task BackupDatabase(
        [Summary("Path", "The folder to create a backup of the database in")]
        string path)
    {
        if (!CheckRolePermission(configuration))
        {
            await RespondAsync(GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }

        const string fileName = "app.db";

        string dbPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var now = DateTime.UtcNow;
        string backupPath = Path.Combine(path, $"{now:yyyyMMddHHmmss}_{fileName}");

        File.Delete(backupPath);
        File.Copy(dbPath, backupPath);

        await RespondAsync($"Done. The backup is in {backupPath}", ephemeral: true);
    }

    [SlashCommand("restore-database", "Deletes the current database for the backupped one")]
    public async Task Restore(
        [Summary("Path", "The folder where the backup database is")]
        string path)
    {
        if (!CheckRolePermission(configuration))
        {
            await RespondAsync(GetUnauthorizedMessage(configuration), ephemeral: true);
            return;
        }

        const string fileName = "app.db";

        string dbPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var latestBackup = Directory.GetFiles(path)
            .Where(temp => temp.EndsWith("_app.db"))
            .Max(Path.GetFullPath);

        if (latestBackup is null)
        {
            await RespondAsync("No backup file was found in that folder");
            return;
        }

        File.Copy(latestBackup, dbPath, true);

        await RespondAsync($"Done.", ephemeral: true);
    }
}