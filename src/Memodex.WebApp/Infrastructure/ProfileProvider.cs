namespace Memodex.WebApp.Infrastructure;

public record CurrentProfile(
    string AvatarPath,
    string PreferredTheme);

public class ProfileProvider
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

    public async Task<CurrentProfile> GetSelectedProfileAsync()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is null.");
        }

        string? rootPath = _configuration.GetSection("Media")
            .GetSection("Avatars")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(rootPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        }

        string? avatar = _httpContextAccessor.HttpContext.Session.GetString("avatar");
        string? preferredTheme = _httpContextAccessor.HttpContext.Session.GetString("preferredTheme");

        if (avatar is null || preferredTheme is null)
        {
            await using SqliteConnection mdxDbConnection =
                SqliteConnectionFactory.CreateForUser(_httpContextAccessor.HttpContext.User);
            await mdxDbConnection.OpenAsync();
            await using SqliteCommand getPrefsCmd = mdxDbConnection.CreateCommand(
                """
                SELECT key, value
                FROM preferences;
                """);

            Dictionary<string, string> userPrefs = new();
            await using SqliteDataReader prefsReader = await getPrefsCmd.ExecuteReaderAsync();
            while (await prefsReader.ReadAsync())
            {
                userPrefs.Add(prefsReader.GetString(0), prefsReader.GetString(1));
            }

            if (!userPrefs.TryGetValue("avatar", out string? value))
            {
                avatar = "default.png";
                await using SqliteCommand setAvatarCmd = mdxDbConnection.CreateCommand(
                    """
                    INSERT INTO preferences (key, value)
                    VALUES ('avatar', @avatar);
                    """);
                setAvatarCmd.Parameters.AddWithValue("@avatar", avatar);
                await setAvatarCmd.ExecuteNonQueryAsync();
            }
            else
            {
                avatar = value;
            }

            if (!userPrefs.TryGetValue("preferredTheme", out value))
            {
                preferredTheme = "light";
                await using SqliteCommand setThemeCmd = mdxDbConnection.CreateCommand(
                    """
                    INSERT INTO preferences (key, value)
                    VALUES ('preferredTheme', @preferredTheme);
                    """);
                setThemeCmd.Parameters.AddWithValue("@preferredTheme", preferredTheme);
                await setThemeCmd.ExecuteNonQueryAsync();
            }
            else
            {
                preferredTheme = value;
            }

            _httpContextAccessor.HttpContext.Session.SetString("avatar", avatar);
            _httpContextAccessor.HttpContext.Session.SetString("preferredTheme", preferredTheme);
        }

        return new CurrentProfile(
            Path.Combine("media", rootPath, avatar),
            preferredTheme);
    }
}
