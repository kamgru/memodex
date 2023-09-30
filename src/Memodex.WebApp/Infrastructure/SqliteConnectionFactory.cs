using System.Security.Claims;

namespace Memodex.WebApp.Infrastructure;

public static class SqliteConnectionFactory
{
    public static SqliteConnection CreateForUser(
        ClaimsPrincipal principal,
        bool enableForeignKeys = false,
        bool createIfNotExists = false)
    {
        string databaseName = principal.Identity switch
        {
            { Name: null } => "anonymous.db",
            { IsAuthenticated: true } => $"mdx.{principal.Identity.Name.ToLowerInvariant()}.db",
            _ => "anonymous.db"
        };

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseName,
            ForeignKeys = enableForeignKeys,
            Mode = createIfNotExists ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite
        }.ToString();

        return new SqliteConnection(connectionString);
    }

    public static SqliteConnection CreateForApp()
    {
        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = "memodex.db",
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}