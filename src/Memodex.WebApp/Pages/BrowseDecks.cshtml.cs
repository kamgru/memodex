using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class BrowseDecks : PageModel
{
    private readonly IMediator _mediator;

    public BrowseDecks(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        CurrentCategoryId = categoryId;
        Decks = await _mediator.Send(new GetDecksRequest(categoryId));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int deckId)
    {
        int challengeId = await _mediator.Send(new CreateChallenge(deckId));
        return RedirectToPage("Engage", new { challengeId });
    }
    
    public int CurrentCategoryId { get; set; }

    public IEnumerable<DeckItem> Decks { get; set; } = new List<DeckItem>();

    public record DeckItem(
        int Id,
        string Name,
        string Description,
        int ItemCount);

    public record GetDecksRequest(
        int CategoryId) : IRequest<IEnumerable<DeckItem>>;
    
    public record CreateChallenge(
        int DeckId) : IRequest<int>;
        
    public class GetDecksHandler : IRequestHandler<GetDecksRequest, IEnumerable<DeckItem>>
    {
        private readonly MemodexContext _memodexContext;

        public GetDecksHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<IEnumerable<DeckItem>> Handle(
            GetDecksRequest request,
            CancellationToken cancellationToken)
        {
            List<DeckItem> decks = await _memodexContext.Decks
                .Include(deck => deck.Flashcards)
                .Where(deck => deck.CategoryId == request.CategoryId)
                .Select(deck => new DeckItem(
                    deck.Id,
                    deck.Name,
                    deck.Description,
                    deck.Flashcards.Count))
                .ToListAsync(cancellationToken: cancellationToken);

            return decks;
        }
    }
    
    public class CreateChallengeHandler : IRequestHandler<CreateChallenge, int>
    {
        private readonly MemodexContext _memodexContext;
        private readonly IProfileProvider _profileProvider;

        public CreateChallengeHandler(
            MemodexContext memodexContext,
            IProfileProvider profileProvider)
        {
            _memodexContext = memodexContext;
            _profileProvider = profileProvider;
        }

        public async Task<int> Handle(
            CreateChallenge request,
            CancellationToken cancellationToken)
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
                ProfileId = _profileProvider.GetCurrentProfileId() ??
                            throw new InvalidOperationException("Cannot create a challenge without a profile."),
                CurrentStepIndex = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                State = ChallengeState.InProgress
            };

            challenge.ChallengeSteps = flashcards.Select((
                flashcard,
                index) => new ChallengeStep
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