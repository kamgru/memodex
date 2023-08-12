using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class CategoryAddCommandFactory
{
    public static Command Create(Command addDefaultCategoriesCommand)
    {
        Option<string> nameOption = new(new[] { "--name", "-n" }, "The name of the category to add")
        {
            IsRequired = true
        };

        Option<string> descriptionOption =
            new(new[] { "--description", "-d" }, "The description of the category to add")
            {
                IsRequired = true
            };
        
        Option<string> imageFilenameOption =
            new(new[] { "--image-filename", "-i" }, "The filename of the image to use for the category")
            {
                IsRequired = false
            };
        
        Command command = new("add", "Add a category to the database")
        {
            nameOption,
            descriptionOption,
            imageFilenameOption,
            addDefaultCategoriesCommand
        };
        
        command.SetHandler((name, description, filename) =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            memodexContext.Categories.Add(new Category
            {
                Name = name,
                Description = description,
                Decks = new List<Deck>(),
                ImageFilename = string.IsNullOrWhiteSpace(filename) ? "default.png" : filename
            });

            memodexContext.SaveChanges();
        }, nameOption, descriptionOption, imageFilenameOption);

        return command;
    }
}