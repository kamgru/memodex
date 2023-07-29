using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class CategoryListCommandFactory
{
    public static Command Create()
    {
        Command command = new("list", "List all categories in the database");
        command.SetHandler(() =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            foreach (Category category in memodexContext.Categories)
            {
                Console.WriteLine($"{category.Id} - {category.Name} - {category.Description}");
            }
        });

        return command;
    }
}