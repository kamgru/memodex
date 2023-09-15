using System.Security.Claims;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Infrastructure;

public static class SqliteConnectionFactory
{
    public static SqliteConnection Create(
        ClaimsPrincipal principal,
        bool enableForeignKeys = false)
    {
        string? databaseName = principal.Identity switch
        {
            { IsAuthenticated: true } => $"mdx.{principal.Identity.Name}.db",
            _ => "anonymous.db"
        };

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseName,
            ForeignKeys = enableForeignKeys
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
