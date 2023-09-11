using Memodex.WebApp.Common;
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

        await using SqliteConnection connection = new($"Data Source=memodex_test.sqlite");
        const string sql =
            "INSERT INTO categories (`name`, `description`, `imageFilename`) VALUES (@name, @description, @imageFilename);";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@name", Category.Name);
        command.Parameters.AddWithValue("@description", string.Empty);
        command.Parameters.AddWithValue("@imageFilename", "default.png");
        await connection.OpenAsync();
        await command.ExecuteScalarAsync();

        const string sqlLastId = "SELECT last_insert_rowid();";
        SqliteCommand commandLastId = new(sqlLastId, connection);
        int categoryId = Convert.ToInt32(await commandLastId.ExecuteScalarAsync());

        this.AddNotification(NotificationType.Success, $"Category {Category.Name} added.");

        return RedirectToPage("EditCategory", new { categoryId });
    }
}