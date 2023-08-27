using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class AvatarImportCommandFactory
{
    public static Command Create()
    {
        Option<string> pathOption = new(new []{"--path", "-p"}, "The path to the directory to import avatars from.")
        {
            IsRequired = true
        };
        
        Command command = new("import", "Import avatars from a directory.")
        {
            pathOption
        };

        command.SetHandler(path =>
        {
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
        }, pathOption);

        return command;
    }
}