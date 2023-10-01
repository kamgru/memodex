using System.Security.Claims;

namespace Memodex.WebApp.Infrastructure;

public class SqliteConnectionFactory
{
    private readonly MediaPhysicalPath _mediaPhysicalPath;

    public SqliteConnectionFactory(
        MediaPhysicalPath mediaPhysicalPath)
    {
        _mediaPhysicalPath = mediaPhysicalPath;
    }

    public SqliteConnection CreateForUser(
        ClaimsPrincipal principal,
        bool enableForeignKeys = false,
        bool createIfNotExists = false)
    {
        if (principal.Identity?.Name is null)
        {
            throw new InvalidOperationException("Principal identity is null.");
        }

        string databaseName = $"mdx.{principal.Identity.Name.ToLowerInvariant()}.db";
        databaseName = Path.Combine(_mediaPhysicalPath.ToString(), databaseName);

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseName,
            ForeignKeys = enableForeignKeys,
            Mode = createIfNotExists ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite
        }.ToString();

        return new SqliteConnection(connectionString);
    }

    public SqliteConnection CreateForApp()
    {
        string databaseName = Path.Combine(_mediaPhysicalPath.ToString(), "memodex.db");
        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseName,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
