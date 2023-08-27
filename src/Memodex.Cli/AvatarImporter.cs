using Memodex.DataAccess;

namespace Memodex.Cli;

public static class AvatarImporter
{
    public static void ImportDefaultAvatars(string? path = null)
    {
        path ??= "/app/data/img/avatars";
        string[] files = Directory.GetFiles(path);
        MemodexContext memodexContext = MemodexContextFactory.Create();
        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);
                
            if (filename.StartsWith("t_"))
            {
                continue;
            }
                
            memodexContext.Avatars.Add(new Avatar
            {
                ImageFilename = filename
            });
        }

        memodexContext.SaveChanges();
    }
}