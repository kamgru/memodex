using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class BrowseCategories : PageModel
{
    public record CategoryItem(
        int Id,
        string Name,
        string? Description,
        string ImagePath,
        int DeckCount);

    private readonly MediaPathProvider _mediaPathProvider;

    public BrowseCategories(
        MediaPathProvider mediaPathProvider)
    {
        _mediaPathProvider = mediaPathProvider;
    }

    public List<CategoryItem> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        //TODO: remove hardcoded user name

        await using SqliteConnection connection = new($"Data Source=memodex_test.sqlite");
        const string sql = "SELECT `id`, `name`, `description`, `imageFilename`, `deckCount` FROM categories;";

        await using SqliteCommand command = new(sql, connection);
        await connection.OpenAsync();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            CategoryItem categoryItem = new(
                Id: reader.GetInt32(0),
                Name: reader.GetString(1),
                Description: reader[2] as string, 
                ImagePath: _mediaPathProvider.GetCategoryThumbnailPath(reader.GetString(3)),
                DeckCount: reader.GetInt32(4));

            Categories.Add(categoryItem);
        }

        return Page();
    }
}