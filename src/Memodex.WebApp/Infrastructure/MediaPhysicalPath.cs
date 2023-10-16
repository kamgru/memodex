namespace Memodex.WebApp.Infrastructure;

public class MediaPhysicalPath
{
    private static string? _mediaRootPath;
    private readonly IConfiguration _configuration;

    public MediaPhysicalPath(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(_mediaRootPath))
        {
            return _mediaRootPath;
        }

        _mediaRootPath = _configuration.GetSection("Media")
                             .GetValue<string>("Path")
                         ?? throw new InvalidOperationException("Missing configuration for Media:Path.");
        if (!Path.IsPathRooted(_mediaRootPath))
        {
            _mediaRootPath = Path.Combine(Directory.GetCurrentDirectory(), _mediaRootPath);
        }

        return _mediaRootPath;
    }
}
