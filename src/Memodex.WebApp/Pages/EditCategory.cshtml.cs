using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class EditCategory : PageModel
{
    public record CategoryItem
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string ImagePath { get; init; } = string.Empty;
    }

    private readonly MediaPathProvider _mediaPathProvider;
    private readonly StaticFilesPathProvider _staticFilesPathProvider;
    private readonly Thumbnailer _thumbnailer;

    public EditCategory(
        StaticFilesPathProvider staticFilesPathProvider,
        MediaPathProvider mediaPathProvider,
        Thumbnailer thumbnailer)
    {
        _staticFilesPathProvider = staticFilesPathProvider;
        _mediaPathProvider = mediaPathProvider;
        _thumbnailer = thumbnailer;
    }

    [BindProperty]
    public CategoryItem? Category { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            SELECT id, name, description, imageFilename
            FROM categories
            WHERE id = @id;
            """);
        command.Parameters.AddWithValue("@id", categoryId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return RedirectToPage("BrowseCategories");
        }

        Category = new CategoryItem
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.GetValue(2) as string ?? string.Empty,
            ImagePath = _mediaPathProvider.GetCategoryThumbnailPath(reader.GetString(3))
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        IFormFile? formFile)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? newFilename = null;
        if (formFile is not null)
        {
            string filename = Path.GetFileName(formFile.FileName);
            newFilename = $"c_{Guid.NewGuid():N}_{filename}";
            string physicalPath = _staticFilesPathProvider.GetCategoryPhysicalPath(newFilename);
            await using FileStream stream = new(physicalPath, FileMode.Create);
            await formFile.CopyToAsync(stream);
            await _thumbnailer.CreateThumbnailAsync(physicalPath);
        }

        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();

        string sql = newFilename is not null
            ? """
              UPDATE categories
              SET name = @name, description = @description, imageFilename = @imageFilename
              WHERE id = @id;
              """
            : """
              UPDATE categories
              SET name = @name, description = @description
              WHERE id = @id;
              """;
        SqliteCommand command = connection.CreateCommand(sql);
        command.Parameters.AddWithValue("@name", Category!.Name);
        command.Parameters.AddWithValue("@description",
            Category.Description is null ? DBNull.Value : Category.Description);
        if (newFilename is not null)
        {
            command.Parameters.AddWithValue("@imageFilename", newFilename);
        }

        command.Parameters.AddWithValue("@id", Category.Id);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Category {Category.Name} updated.");

        return RedirectToPage("EditCategory", new { categoryId = Category.Id });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(
        [FromQuery]
        int categoryId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            DELETE FROM categories
             WHERE id = @id;
            """);
        command.Parameters.AddWithValue("@id", categoryId);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Category {Category!.Name} deleted.");

        return RedirectToPage("BrowseCategories");
    }
}
