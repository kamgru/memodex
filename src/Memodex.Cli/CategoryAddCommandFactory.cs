using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class CategoryAddCommandFactory
{
    public static Command Create()
    {
        var nameOption = new Option<string>(new[] { "--name", "-n" }, "The name of the category to add")
        {
            IsRequired = true
        };

        var descriptionOption =
            new Option<string>(new[] { "--description", "-d" }, "The description of the category to add")
            {
                IsRequired = true
            };
        var command = new Command("add", "Add a category to the database")
        {
            nameOption,
            descriptionOption
        };

        command.SetHandler((name, description) =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            memodexContext.Categories.Add(new Category
            {
                Name = name,
                Description = description,
                Decks = new List<Deck>()
            });
            
            memodexContext.SaveChanges();
        }, nameOption, descriptionOption);

        return command;
    }
}