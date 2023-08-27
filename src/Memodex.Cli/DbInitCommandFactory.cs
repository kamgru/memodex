using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DbInitCommandFactory
{
    public static Command Create()
    {
        Command command = new("init", "Initialize the database.");

        command.SetHandler(() =>
        {
            string connectionString = ConnectionStringManager.GetConnectionString().Value;
            MemodexContext memodexContext = MemodexContextFactory.Create(connectionString);
            memodexContext.Database.EnsureCreated();
        });
        
        return command;
    }
}