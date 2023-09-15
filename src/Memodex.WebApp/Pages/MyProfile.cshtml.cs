using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class MyProfile : PageModel
{
    public record UpdateTheme(
        int ProfileId,
        string Theme);

    public async Task<IActionResult> OnPostUpdateThemeAsync(
        [FromBody]
        UpdateTheme updateTheme)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            UPDATE profiles
            SET preferredTheme = @theme
            WHERE id = @id;
            """);
        command.Parameters.AddWithValue("@id", updateTheme.ProfileId);
        command.Parameters.AddWithValue("@theme", updateTheme.Theme);
        await command.ExecuteNonQueryAsync();

        return new OkResult();
    }
}
