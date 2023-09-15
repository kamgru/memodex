using System.Data.Common;
using System.Text.Json;
using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class ImportDeck : PageModel
{
    public record FlashcardItem(
        string Question,
        string Answer);

    public class DeckItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IEnumerable<FlashcardItem> Flashcards { get; set; } = new List<FlashcardItem>();
    }

    [BindProperty]
    public int CategoryId { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        CategoryId = categoryId;
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT EXISTS(
                SELECT 1
                FROM categories
                WHERE Id = @categoryId)
            """);
        command.Parameters.AddWithValue("@categoryId", categoryId);

        bool categoryExists = Convert.ToBoolean(await command.ExecuteScalarAsync());
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


        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        await using SqliteCommand addDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description, flashcardCount, categoryId)
            VALUES (@name, @description, @flashcardCount, @categoryId)
            RETURNING id;
            """);
        addDeckCmd.Parameters.AddWithValue("name", deckItem.Name);
        addDeckCmd.Parameters.AddWithValue("description",
            deckItem.Description is null ? DBNull.Value : deckItem.Description);
        addDeckCmd.Parameters.AddWithValue("flashcardCount", deckItem.Flashcards.Count());
        addDeckCmd.Parameters.AddWithValue("categoryId", CategoryId);

        int deckId = Convert.ToInt32(await addDeckCmd.ExecuteScalarAsync());

        await using SqliteCommand addFlashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES (@question, @answer, @deckId);
            """);
        foreach (FlashcardItem flashcardItem in deckItem.Flashcards)
        {
            addFlashcardCmd.Parameters.Clear();
            addFlashcardCmd.Parameters.AddWithValue("question", flashcardItem.Question);
            addFlashcardCmd.Parameters.AddWithValue("answer", flashcardItem.Answer);
            addFlashcardCmd.Parameters.AddWithValue("deckId", deckId);

            await addFlashcardCmd.ExecuteNonQueryAsync();
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
}
