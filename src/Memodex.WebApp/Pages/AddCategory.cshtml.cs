using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class AddCategory : PageModel
{
    public record CategoryItem(
        string Name = "");

    [BindProperty]
    public CategoryItem Category { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        
        SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO categories (`name`, `description`, `imageFilename`) 
            VALUES (@name, @description, @imageFilename)
            RETURNING `id`;
            """);
        command.Parameters.AddWithValue("@name", Category.Name);
        command.Parameters.AddWithValue("@description", string.Empty);
        command.Parameters.AddWithValue("@imageFilename", "default.png");

        object scalar = await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("Scalar is null.");
        int categoryId = Convert.ToInt32(scalar);

        this.AddNotification(NotificationType.Success, $"Category {Category.Name} added.");

        return RedirectToPage("EditCategory", new { categoryId });
    }
}