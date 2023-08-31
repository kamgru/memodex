using Microsoft.Extensions.Configuration;

namespace Memodex.Cli;

public class InitCommand : Command
{
    public InitCommand()
        : base("init", "Initialize Memodex")
    {
        this.SetHandler(Handle);
    }
    
    private void Handle(
        InvocationContext invocationContext)
    {
        IConfiguration configuration = invocationContext.GetRequiredService<IConfiguration>();
        
        MemodexContextFactory memodexContextFactory = invocationContext.GetRequiredService<MemodexContextFactory>();
        MemodexContext memodexContext = memodexContextFactory.Create();
        memodexContext.Database.EnsureCreated();

        HashSet<string> existingAvatarFiles = memodexContext.Avatars
            .Select(avatar => avatar.ImageFilename)
            .ToHashSet();

        string dataDirectory = configuration.GetSection("Media").GetValue<string>("Path")
            ?? throw new InvalidOperationException("Missing configuration for Media:Path.");
        string avatarSubDirectory = configuration.GetSection("Media").GetSection("Avatars").GetValue<string>("Path")
            ?? throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
        string avatarDirectory = Path.Combine(dataDirectory, avatarSubDirectory);
        
        string[] avatarFiles = Directory.GetFiles(avatarDirectory, "*.png")
            .Select(file => Path.GetFileName(file))
            .Where(file => Regex.IsMatch(file, "^[^t_][\\S]*[.png]$"))
            .ToArray();

        foreach (string avatarFile in avatarFiles)
        {
            if (existingAvatarFiles.Contains(avatarFile))
            {
                continue;
            }
            
            Avatar avatar = new()
            {
                ImageFilename = avatarFile,
            };
            
            memodexContext.Avatars.Add(avatar);
        }
        memodexContext.SaveChanges();
    }
}