using Microsoft.Extensions.Configuration;

namespace Memodex.Cli;

public class CategoryCommand : Command
{
    public CategoryCommand()
        : base("category", "Manage categories")
    {
        AddCommand(new CategoryListCommand());
        AddCommand(new CategoryAddCommand());
        AddCommand(new CategoryImportDefaultCommand());
    }
}

public class CategoryListCommand : Command
{
    public CategoryListCommand()
        : base("list", "List all categories")
    {
        this.SetHandler(Handle);
    }

    private void Handle(
        InvocationContext invocationContext)
    {
        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();

        List<Category> categories = memodexContext.Categories
            .ToList();

        Console.WriteLine("{0,-10} {1,-15} {2,-30}", "Id", "Name", "Description");
        Console.WriteLine("{0,-10} {1,-15} {2,-30}", "--", "----", "-----------");
        foreach (Category category in categories)
        {
            Console.WriteLine("{0,-10} {1,-15} {2,-30}", category.Id, category.Name, category.Description);
        }
    }
}

public class CategoryAddCommand : Command
{
    private readonly Option<string> _nameOption = new("--name")
    {
        Description = "The name of the category to add",
        IsRequired = true
    };

    private readonly Option<string> _descriptionOption = new("--description")
    {
        Description = "The description of the category to add",
        IsRequired = true
    };

    private readonly Option<string> _imageFilenameOption = new("--image-filename")
    {
        Description = "The image filename of the category to add",
        IsRequired = true
    };

    public CategoryAddCommand()
        : base("add", "Add a new category")
    {
        AddOption(_nameOption);
        AddOption(_descriptionOption);
        AddOption(_imageFilenameOption);
        this.SetHandler(Handle);
    }

    private void Handle(
        InvocationContext invocationContext)
    {
        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();

        string name = invocationContext.GetOptionValue(_nameOption);
        string description = invocationContext.GetOptionValue(_descriptionOption);
        string imageFilename = invocationContext.GetOptionValue(_imageFilenameOption);

        Category category = new()
        {
            Name = name,
            Description = description,
            Decks = new List<Deck>(),
            ImageFilename = imageFilename
        };
        memodexContext.Categories.Add(category);
        memodexContext.SaveChanges();

        Console.WriteLine("{0,-10} {1,-15} {2,-30}", "Id", "Name", "Description");
        Console.WriteLine("{0,-10} {1,-15} {2,-30}", "--", "----", "-----------");
        Console.WriteLine("{0,-10} {1,-15} {2,-30}", category.Id, category.Name, category.Description);
    }
}

public class CategoryImportDefaultCommand : Command
{
    private class FlashcardData
    {
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }

    private class DeckData
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<FlashcardData> Flashcards { get; set; }
    }

    private class CategoryData
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Image { get; set; }
        public required string Source { get; set; }
        public List<DeckData> Decks { get; set; } = new();
    }

    public CategoryImportDefaultCommand()
        : base("import-default", "Import default categories")
    {
        this.SetHandler(Handle);
    }

    private void Handle(
        InvocationContext invocationContext)
    {

        IConfiguration configuration = invocationContext.GetRequiredService<IConfiguration>();

        string dataDirectoryPath = configuration.GetSection("data")
                                       .GetValue<string>("Path")
                                   ?? throw new InvalidOperationException("Missing configuration for data directory");

        string defaultCategoriesRelativePath = configuration.GetSection("data:Categories")
                                                   .GetValue<string>("Path")
                                               ?? throw new InvalidOperationException(
                                                   "Missing configuration for default categories");

        string defaultCategoriesFilename = Path.Combine(dataDirectoryPath, defaultCategoriesRelativePath);

        string jsonContent = File.ReadAllText(defaultCategoriesFilename);
        List<CategoryData> categoryDataList = JsonSerializer.Deserialize<List<CategoryData>>(jsonContent,
                                                  new JsonSerializerOptions()
                                                  {
                                                      PropertyNameCaseInsensitive = true
                                                  })
                                              ?? throw new InvalidOperationException(
                                                  "Failed to deserialize default categories");

        string defaultCategoriesDirectory = Path.GetDirectoryName(defaultCategoriesFilename)!;
        foreach (CategoryData categoryData in categoryDataList)
        {
            string deckDirectoryName = Path.Combine(defaultCategoriesDirectory, categoryData.Source);
            
            foreach (string deckFilename in Directory.GetFiles(deckDirectoryName))
            {
                string deckJsonContent = File.ReadAllText(deckFilename);
                DeckData deckData = JsonSerializer.Deserialize<DeckData>(deckJsonContent,
                                        new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        })
                                    ?? throw new InvalidOperationException(
                                        "Failed to deserialize default deck");
                categoryData.Decks.Add(deckData);
            }

        }

        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();
        
        foreach (CategoryData categoryData in categoryDataList)
        {
            Category category = new()
            {
                Name = categoryData.Name,
                Description = categoryData.Description,
                ImageFilename = categoryData.Image,
                Decks = categoryData.Decks.Select(deckData => new Deck
                    {
                        Name = deckData.Name,
                        Description = deckData.Description,
                        Flashcards = deckData.Flashcards.Select(flashcardData => new Flashcard
                            {
                                Question = flashcardData.Question,
                                Answer = flashcardData.Answer
                            })
                            .ToList(),
                        ItemCount = deckData.Flashcards.Count
                    })
                    .ToList(),
                ItemCount = categoryData.Decks.Count
            };
            memodexContext.Categories.Add(category);
        }

        memodexContext.SaveChanges();
    }
}