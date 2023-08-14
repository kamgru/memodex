namespace Memodex.WebApp.Infrastructure;

public class MediaPathProvider
{
    private readonly IConfiguration _configuration;

    public MediaPathProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetAvatarThumbnailPath(string avatarFilename)
    {
        string? avatarPath = _configuration.GetSection("Media")
            .GetSection("Avatars")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(avatarPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        }

        return Path.Combine("media", avatarPath, $"t_{avatarFilename}");
    }
    
    public string GetCategoryThumbnailPath(string categoryThumbnailFilename)
    {
        string? categoryPath = _configuration.GetSection("Media")
            .GetSection("Categories")
            .GetValue<string>("Path");

        if (string.IsNullOrEmpty(categoryPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Categories:Path.");
        }

        return Path.Combine("media", categoryPath, $"t_{categoryThumbnailFilename}");
    }
}