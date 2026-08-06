using DailyRugby.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = new SqliteConnection("Filename=:memory:");
connection.Open();
builder.Services.AddSingleton(connection);

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

app.MapGet("/", () => "Hello World!");
app.Run();