using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DbCommandHandler
{
    public static void HandleDbSet(string host, string db, string user, string password, bool init = false)
    {
        string connectionString = $"Server={host};Database={db};User Id={user};Password={password};TrustServerCertificate=True";

        ConnectionStringManager.SetConnectionString(new ConnectionString(connectionString));

        if (init)
        {
            MemodexContext memodexContext = MemodexContextFactory.Create(connectionString);
            memodexContext.Database.EnsureCreated();
            AvatarImporter.ImportDefaultAvatars();
        }
    }

    public static void HandleDbShow()
    {
        string connectionString = ConnectionStringManager.GetConnectionString().Value;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("No connection string found. Please set one using the 'db set' command.");
        }
        Console.WriteLine(connectionString);
    }

    public static void HandleDbWipe()
    {
        MemodexContext memodexContext = MemodexContextFactory.Create();
        memodexContext.Database.EnsureDeleted();
        memodexContext.Database.EnsureCreated();
    }
}