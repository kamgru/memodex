using System.Data.Common;
using System.Text.Json;
using Memodex.WebApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class ImportDeck : PageModel
{
    [BindProperty]
    public int CategoryId { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        CategoryId = categoryId;
        await using SqliteConnection connection = new("Data Source=memodex_test.sqlite");
        await connection.OpenAsync();
        const string sql = "SELECT EXISTS(SELECT 1 FROM categories WHERE Id = @categoryId)";
        await using SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@categoryId", categoryId);

        bool categoryExists = Convert.ToInt32(await command.ExecuteScalarAsync() ?? 0) == 1;

        if (!categoryExists)
        {
            return RedirectToPage("BrowseCategories");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        IFormFile? formFile)
    {
        if (formFile is null)
        {
            this.AddNotification(NotificationType.Error, "No file selected");
            return Page();
        }

        await using Stream stream = formFile.OpenReadStream();

        DeckItem deckItem = await JsonSerializer.DeserializeAsync<DeckItem>(
                                stream,
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                })
                            ?? throw new InvalidOperationException("Invalid JSON file");


        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        const string insertDeckSql =
            """
            INSERT INTO decks (name, description, flashcardCount, categoryId)
            VALUES (@name, @description, @flashcardCount, @categoryId)
            """;

        SqliteCommand insertDeckCommand = connection.CreateCommand();
        insertDeckCommand.CommandText = insertDeckSql;
        insertDeckCommand.Parameters.AddWithValue("name", deckItem.Name);
        insertDeckCommand.Parameters.AddWithValue("description",
            deckItem.Description is null ? DBNull.Value : deckItem.Description);
        insertDeckCommand.Parameters.AddWithValue("flashcardCount", deckItem.Flashcards.Count());
        insertDeckCommand.Parameters.AddWithValue("categoryId", CategoryId);

        await insertDeckCommand.ExecuteNonQueryAsync();
        const string selectLastIdSql = "SELECT last_insert_rowid();";
        SqliteCommand commandLastId = connection.CreateCommand();
        commandLastId.CommandText = selectLastIdSql;

        int deckId = Convert.ToInt32(await commandLastId.ExecuteScalarAsync());

        SqliteCommand insertFlashcardCommand = connection.CreateCommand();
        insertFlashcardCommand.CommandText =
            "INSERT INTO flashcards (question, answer, deckId) VALUES (@question, @answer, @deckId)";
        foreach (FlashcardItem flashcardItem in deckItem.Flashcards)
        {
            insertFlashcardCommand.Parameters.Clear();
            insertFlashcardCommand.Parameters.AddWithValue("question", flashcardItem.Question);
            insertFlashcardCommand.Parameters.AddWithValue("answer", flashcardItem.Answer);
            insertFlashcardCommand.Parameters.AddWithValue("deckId", deckId);

            await insertFlashcardCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = deckId
            },
            ViewName = "_ImportDeckSuccessPartial"
        };
    }

    public record FlashcardItem(
        string Question,
        string Answer);

    public class DeckItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IEnumerable<FlashcardItem> Flashcards { get; set; } = new List<FlashcardItem>();
    }
}