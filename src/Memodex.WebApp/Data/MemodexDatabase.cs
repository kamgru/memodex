using System.Data.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Memodex.WebApp.Data;

public class AddUserResult
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public MdxUser? User { get; }

    private AddUserResult(
        string? errorMessage, MdxUser? mdxUser = null)
    {
        ErrorMessage = errorMessage ?? string.Empty;
        IsSuccess = errorMessage is null;
        User = mdxUser;
    }

    public static AddUserResult Success(MdxUser mdxUser) => new(null, mdxUser);

    public static AddUserResult Failure(
        string errorMessage) => new(errorMessage);
}

public class MemodexDatabase
{
    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public MemodexDatabase(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public async Task EnsureExistsAsync()
    {
        await using SqliteConnection mdxDbConnection = _sqliteConnectionFactory.CreateForApp();
        await mdxDbConnection.OpenAsync();
        await using SqliteCommand createDbCmd = mdxDbConnection.CreateCommand(
            """
            create table if not exists users
            (
                id              integer not null
                    constraint users_pk
                        primary key autoincrement,
                username        varchar(255) not null,
                userId          varchar(255) not null,
                passwordHash    varchar(255) not null
            );
            """);
        await createDbCmd.ExecuteNonQueryAsync();
        await mdxDbConnection.CloseAsync();
    }

    public async Task<AddUserResult> AddUserAsync(
        string username,
        string password)
    {
        await using SqliteConnection mdxDbConnection = _sqliteConnectionFactory.CreateForApp();
        await mdxDbConnection.OpenAsync();
        await using DbTransaction mdxDbTransaction = await mdxDbConnection.BeginTransactionAsync();

        await using SqliteCommand userExistsCmd = mdxDbConnection.CreateCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM users
                WHERE username = @username
            );
            """);
        string usernameNormalized = username.ToLowerInvariant();
        userExistsCmd.Parameters.AddWithValue("@username", usernameNormalized);

        bool userExists = Convert.ToBoolean(await userExistsCmd.ExecuteScalarAsync());
        if (userExists)
        {
            return AddUserResult.Failure($"User {usernameNormalized} already exists.");
        }

        MdxUser user = new()
        {
            Username = usernameNormalized,
            UserId = Guid.NewGuid()
                .ToString()
        };
        PasswordHasher<MdxUser> hasher = new();
        user.PasswordHash = hasher.HashPassword(user, password);

        await using SqliteCommand addUserCmd = mdxDbConnection.CreateCommand(
            """
            INSERT INTO users (userId, username, passwordHash)
            VALUES (@userId, @username, @passwordHash);
            """);
        addUserCmd.Parameters.AddWithValue("@userId", user.UserId);
        addUserCmd.Parameters.AddWithValue("@username", user.Username);
        addUserCmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        await addUserCmd.ExecuteNonQueryAsync();
        await mdxDbTransaction.CommitAsync();
        await mdxDbConnection.CloseAsync();

        return AddUserResult.Success(user);
    }

    public async Task<MdxUser?> GetUserAsync(
        string username)
    {
        await using SqliteConnection mdxDbConnection = _sqliteConnectionFactory.CreateForApp();
        await mdxDbConnection.OpenAsync();
        await using SqliteCommand getUserCmd = mdxDbConnection.CreateCommand(
            """
            SELECT userId, username, passwordHash
            FROM users
            WHERE username = @username
            LIMIT 1;
            """);
        string usernameNormalized = username.ToLowerInvariant();
        getUserCmd.Parameters.AddWithValue("@username", usernameNormalized);
        
        await using SqliteDataReader reader = await getUserCmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }
        
        MdxUser user = new()
        {
            UserId = reader.GetString(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2)
        };
        
        await mdxDbConnection.CloseAsync();
        return user;
    }

    public async Task DeleteUserAsync(
        string userId)
    {
        await using SqliteConnection connection = _sqliteConnectionFactory.CreateForApp();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            DELETE FROM users
            WHERE userId = @userId;
            """);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }
}
