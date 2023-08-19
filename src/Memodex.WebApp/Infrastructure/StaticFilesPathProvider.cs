namespace Memodex.WebApp.Infrastructure;

public class StaticFilesPathProvider
{
    private readonly IConfiguration _configuration;

    public StaticFilesPathProvider(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetCategoryPhysicalPath(
        string categoryFilename)
    {
        IConfigurationSection mediaSection = _configuration.GetSection("Media");
        string rootPath = mediaSection.GetValue<string>("Path")
                          ?? throw new InvalidOperationException("Missing configuration for Media:Path.");

        string? categoryPath = mediaSection.GetSection("Categories").GetValue<string>("Path");

        if (string.IsNullOrEmpty(categoryPath))
        {
            throw new InvalidOperationException("Missing configuration for Media:Categories:Path.");
        }

        return Path.Combine(rootPath, categoryPath, categoryFilename);
    }
}