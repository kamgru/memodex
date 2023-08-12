using System.CommandLine;
using System.Text.Json;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DeckImportCommandFactory
{
    public static Command Create()
    {
        Option<string> pathOption = new(
            new[] { "--path", "-p" },
            description: "The path of the deck to import"
        )
        {
            IsRequired = true
        };

        Option<int> categoryIdOption = new(
            new[] { "--category-id", "-c" },
            description: "The category id of the deck to import"
        )
        {
            IsRequired = true
        };

        Command command = new("import", "Import a deck to the database")
        {
            categoryIdOption,
            pathOption,
        };

        command.SetHandler((categoryId, inputPath) =>
        {
            if (File.Exists(inputPath))
            {
                DeckJsonImporter.Import(inputPath, categoryId);
                return;
            }

            if (Directory.Exists(inputPath))
            {
                DeckJsonImporter.ImportMany(inputPath, categoryId);
            }
        }, categoryIdOption, pathOption);
        return command;
    }
}