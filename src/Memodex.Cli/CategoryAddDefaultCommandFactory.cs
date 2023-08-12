using System.CommandLine;
using System.Text.Json;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class CategoryAddDefaultCommandFactory
{
    private record CategoryData(string Name, string Description, string Image);
    
    public static Command Create()
    {
        
        Option<string> defaultDataFilenameOption = new(new[] { "--filename", "-f" },
            "The filename of the default data to add to the database")
        {
            IsRequired = true
        };

        Command command = new("default", "Add default categories to the database")
        {
            defaultDataFilenameOption
        };

        command.SetHandler((filename) =>
        {
            string jsonData = File.ReadAllText(filename);
            List<CategoryData> categoryData = JsonSerializer.Deserialize<List<CategoryData>>(jsonData, new JsonSerializerOptions
                                              {
                                                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                              })
                                              ?? throw new InvalidOperationException("Unable to deserialize category data");

            MemodexContext memodexContext = MemodexContextFactory.Create();
            foreach (CategoryData item in categoryData)
            {
                memodexContext.Categories.Add(new Category
                {   
                    Name = item.Name,
                    Description = item.Description,
                    Decks = new List<Deck>(),
                    ImageFilename = item.Image
                });
            }

            memodexContext.SaveChanges();

        }, defaultDataFilenameOption);

        return command;
    }
}