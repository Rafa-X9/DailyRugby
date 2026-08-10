using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Championship> Championships { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<TeamGame> TeamGames { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Championship>().ToTable(nameof(Championships));
        modelBuilder.Entity<Team>().ToTable(nameof(Teams));
        modelBuilder.Entity<Game>().ToTable(nameof(Games));
        modelBuilder.Entity<TeamGame>().ToTable(nameof(TeamGames));
        modelBuilder.Entity<Schedule>().ToTable(nameof(Schedules));

        modelBuilder.Entity<Championship>()
            .Property(temp => temp.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Team>()
            .Property(temp => temp.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Game>()
            .Property(temp => temp.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<TeamGame>()
            .Property(temp => temp.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Championship>(builder =>
        {
            builder.HasMany(champ => champ.Teams).WithOne();

            builder.HasMany(champ => champ.Games)
                .WithOne()
                .HasForeignKey(game => game.ChampionshipId);
        });

        modelBuilder.Entity<Schedule>(builder =>
        {
            builder.HasOne(schedule => schedule.Game)
                .WithOne();
        });
    }
}