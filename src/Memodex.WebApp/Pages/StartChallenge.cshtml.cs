using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class StartChallenge : PageModel
{
    private readonly IMediator _mediator;

    public StartChallenge(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IEnumerable<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
    public IEnumerable<DeckItem> Decks { get; set; } = new List<DeckItem>();
    public int? SelectedCategoryId { get; set; }

    public async Task<IActionResult> OnGetAsync(int? categoryId, int? deckId)
    {
        if (categoryId is null)
        {
            Categories = await _mediator.Send(new GetCategoryItems());
        }
        else
        {
            SelectedCategoryId = categoryId;
            Decks = await _mediator.Send(new GetDeckItems(categoryId.Value));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int deckId)
    {
        int challengeId = await _mediator.Send(new CreateChallenge(deckId));
        return RedirectToPage("Engage", new { challengeId });
    }

    public record CategoryItem(int Id, string Name, string Description, string Image);

    public record DeckItem(int Id, string Name, string Description, int ItemCount);

    public record GetCategoryItems : IRequest<IEnumerable<CategoryItem>>;

    public record GetDeckItems(int CategoryId) : IRequest<IEnumerable<DeckItem>>;

    public record CreateChallenge(int DeckId) : IRequest<int>;

    public class GetCategoryItemsHandler : IRequestHandler<GetCategoryItems, IEnumerable<CategoryItem>>
    {
        private readonly MemodexContext _memodexContext;
        private readonly MediaPathProvider _mediaPathProvider;

        public GetCategoryItemsHandler(MemodexContext memodexContext, MediaPathProvider mediaPathProvider)
        {
            _memodexContext = memodexContext;
            _mediaPathProvider = mediaPathProvider;
        }

        public async Task<IEnumerable<CategoryItem>> Handle(
            GetCategoryItems request,
            CancellationToken cancellationToken)
        {
            List<CategoryItem> result = await _memodexContext.Categories
                .Include(category => category.Decks)
                .ThenInclude(deck => deck.Flashcards)
                .Select(category => new CategoryItem(
                    category.Id,
                    category.Name,
                    category.Description,
                    _mediaPathProvider.GetCategoryThumbnailPath(category.ImageFilename)))
                .ToListAsync(cancellationToken);

            return result;
        }
    }

    public class GetDeckItemsHandler : IRequestHandler<GetDeckItems, IEnumerable<DeckItem>>
    {
        private readonly MemodexContext _memodexContext;

        public GetDeckItemsHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<IEnumerable<DeckItem>> Handle(GetDeckItems request, CancellationToken cancellationToken)
        {
            List<DeckItem> deckItems = await _memodexContext.Decks
                .Include(deck => deck.Flashcards)
                .Where(deck => deck.CategoryId == request.CategoryId)
                .Select(deck => new DeckItem(
                    deck.Id,
                    deck.Name,
                    deck.Description,
                    deck.Flashcards.Count))
                .ToListAsync(cancellationToken);

            return deckItems;
        }
    }

    public class CreateChallengeHandler : IRequestHandler<CreateChallenge, int>
    {
        private readonly MemodexContext _memodexContext;

        public CreateChallengeHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<int> Handle(CreateChallenge request, CancellationToken cancellationToken)
        {
            List<Flashcard> flashcards = await _memodexContext.Flashcards
                .Where(x => x.DeckId == request.DeckId)
                .ToListAsync(cancellationToken);

            if (!flashcards.Any())
            {
                throw new InvalidOperationException("Cannot create a challenge with no flashcards.");
            }

            Challenge challenge = new()
            {
                DeckId = request.DeckId,
                ProfileId = 1,
                CurrentStepIndex = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                State = ChallengeState.InProgress
            };

            challenge.ChallengeSteps = flashcards.Select((flashcard, index) => new ChallengeStep
            {
                Index = index,
                FlashcardId = flashcard.Id,
                Challenge = challenge
            }).ToList();
            
            _memodexContext.Challenges.Add(challenge);
            await _memodexContext.SaveChangesAsync(cancellationToken);

            return challenge.Id;
        }
    }
}