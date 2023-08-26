using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class ProfileCreateDefaultCommandFactory
{
    public static Command Create()
    {
        Command command = new("create-default", "Create a default profile");

        command.SetHandler(() =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            memodexContext.Profiles.Add(new Profile
            {
                Name = "Memodexer",
                AvatarPath = "default.png",
                PreferredTheme = "light"
            });
            memodexContext.SaveChanges();
        });
        
        return command;
    }
}