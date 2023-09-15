using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Infrastructure;

public record CurrentProfile(
    int Id,
    string Name,
    string AvatarPath,
    string PreferredTheme);

public interface IProfileProvider
{
    Task<CurrentProfile?> GetSelectedProfileAsync();
    int? GetCurrentProfileId();
}

public class ProfileProvider : IProfileProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public ProfileProvider(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<CurrentProfile?> GetSelectedProfileAsync()
    {
        string? rootPath = _configuration.GetSection("Media")
            .GetSection("Avatars")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(rootPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        }

        HttpContext context = _httpContextAccessor.HttpContext
                              ?? throw new InvalidOperationException("HttpContext is null.");

        int? profileId = context.Session.GetInt32(Common.Constants.SelectedProfileSessionKey);
        if (profileId == null)
        {
            return null;
        }

        await using SqliteConnection connection = new("data source=memodex.db");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT `id`, `name`, `avatar`, `preferredTheme` from profiles
            WHERE id = @profileId
            LIMIT 1;
            """);
        command.Parameters.AddWithValue("@profileId", profileId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("Profile not found.");
        }

        return new CurrentProfile(
            reader.GetInt32(0),
            reader.GetString(1),
            Path.Combine("media", rootPath, $"t_{reader.GetString(2)}"),
            reader.GetString(3));
    }

    public int? GetCurrentProfileId()
    {
        HttpContext context = _httpContextAccessor.HttpContext
                              ?? throw new InvalidOperationException("HttpContext is null.");

        return context.Session.GetInt32(Common.Constants.SelectedProfileSessionKey);
    }
}
