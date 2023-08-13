using Memodex.DataAccess;

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
    private readonly MemodexContext _memodexContext;
    private readonly IConfiguration _configuration;

    public ProfileProvider(
        IHttpContextAccessor httpContextAccessor,
        MemodexContext memodexContext,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _memodexContext = memodexContext;
        _configuration = configuration;
    }

    public async Task<CurrentProfile?> GetSelectedProfileAsync()
    {
        HttpContext context = _httpContextAccessor.HttpContext
                              ?? throw new InvalidOperationException("HttpContext is null.");

        int? profileId = context.Session.GetInt32(Common.Constants.SelectedProfileSessionKey);
        if (profileId == null)
        {
            return null;
        }

        Profile profile = await _memodexContext.Profiles.FindAsync(profileId)
                          ?? throw new InvalidOperationException("Profile not found.");

        string? rootPath = _configuration.GetSection("Media")
            .GetSection("Avatars")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(rootPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        }

        CurrentProfile currentProfile = new(
            profile.Id,
            profile.Name,
            Path.Combine("media", rootPath, $"t_{profile.AvatarPath}"),
            profile.PreferredTheme);

        return currentProfile;
    }

    public int? GetCurrentProfileId()
    {
        HttpContext context = _httpContextAccessor.HttpContext
                              ?? throw new InvalidOperationException("HttpContext is null.");

        return context.Session.GetInt32(Common.Constants.SelectedProfileSessionKey);
    }
}