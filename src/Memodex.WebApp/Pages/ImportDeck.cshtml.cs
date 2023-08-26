using System.Text.Json;
using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class ImportDeck : PageModel
{
    private readonly IMediator _mediator;

    public ImportDeck(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public int CategoryId { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        CategoryId = categoryId;
        bool categoryExists = await _mediator.Send(new CheckCategoryExistsRequest(categoryId));

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

        int deckId = await _mediator.Send(new ImportDeckRequest(CategoryId, formFile));

        return new PartialViewResult()
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

    public record DeckItem(
        string Name,
        string Description,
        IEnumerable<FlashcardItem> Flashcards);

    public record CheckCategoryExistsRequest(
        int CategoryId) : IRequest<bool>;

    public record ImportDeckRequest(
        int CategoryId,
        IFormFile FormFile) : IRequest<int>;

    public class CheckCategoryExistsHandler : IRequestHandler<CheckCategoryExistsRequest, bool>
    {
        private readonly MemodexContext _memodexContext;

        public CheckCategoryExistsHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<bool> Handle(
            CheckCategoryExistsRequest request,
            CancellationToken cancellationToken)
        {
            bool categoryExists = await _memodexContext.Categories
                .AnyAsync(category => category.Id == request.CategoryId, cancellationToken);

            return categoryExists;
        }
    }

    public class ImportDeckHandler : IRequestHandler<ImportDeckRequest, int>
    {
        private readonly MemodexContext _memodexContext;

        public ImportDeckHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<int> Handle(
            ImportDeckRequest request,
            CancellationToken cancellationToken)
        {
            await using Stream stream = request.FormFile.OpenReadStream();

            DeckItem deckItem =
                await JsonSerializer.DeserializeAsync<DeckItem>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Invalid JSON file");


            Category category = await _memodexContext.Categories
                                    .FirstOrDefaultAsync(item => item.Id == request.CategoryId, cancellationToken)
                                ?? throw new InvalidOperationException($"Category {request.CategoryId} not found");

            Deck deck = new Deck
            {
                Description = deckItem.Description,
                Name = deckItem.Name,
                Flashcards = deckItem.Flashcards.Select(flashcard => new Flashcard
                    {
                        Answer = flashcard.Answer,
                        Question = flashcard.Question
                    })
                    .ToList(),
                CategoryId = category.Id
            };

            _memodexContext.Decks.Add(deck);
            await _memodexContext.SaveChangesAsync(cancellationToken);

            return deck.Id;
        }
    }
}