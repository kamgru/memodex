using SixLabors.ImageSharp.Formats.Png;

namespace Memodex.WebApp.Infrastructure;

public class Thumbnailer
{
    private readonly StaticFilesPathProvider _staticFilesPathProvider;

    public Thumbnailer(
        StaticFilesPathProvider staticFilesPathProvider)
    {
        _staticFilesPathProvider = staticFilesPathProvider;
    }

    public async Task CreateThumbnailAsync(string imagePath)
    {
        using Image image = await Image.LoadAsync(imagePath);
    
        ResizeOptions options = new()
        {
            Size = new Size(120),
            Mode = ResizeMode.Max
        };
        image.Mutate(x => x.Resize(options));
    
        string thumbFilename = $"t_{Path.GetFileName(imagePath)}";
        string physicalPath = Path.Combine(_staticFilesPathProvider.GetCategoryPhysicalPath(thumbFilename)); 
        await using FileStream thumbStream = File.Create(physicalPath);
        await image.SaveAsync(thumbStream, new PngEncoder());
    }
}