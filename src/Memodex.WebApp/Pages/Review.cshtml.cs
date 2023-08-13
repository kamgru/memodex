using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class Review : PageModel
{
    private readonly IMediator _mediator;

    public Review(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public FlashcardItem? CurrentFlashcard { get; set; }

    [BindProperty]
    public StepInput? Input { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        CurrentFlashcard = await _mediator.Send(new GetNextFlashcard(challengeId));
        Input = new StepInput(
            challengeId,
            CurrentFlashcard.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input is null)
        {
            throw new InvalidOperationException("Input is null");
        }

        SubmitResult result = await _mediator.Send(new SubmitFlashcard(
            Input.ChallengeId,
            Input.FlashcardId));

        return result.IsFinished
            ? RedirectToPage("CompleteChallenge", new { challengeId = Input.ChallengeId })
            : RedirectToPage("Review", new { challengeId = Input.ChallengeId });
    }

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer,
        string DeckTitle,
        int DeckItemCount,
        int CurrentStep);

    public record StepInput(
        int ChallengeId,
        int FlashcardId);

    public record SubmitFlashcard(
        int ChallengeId,
        int FlashcardId) : IRequest<SubmitResult>;

    public record SubmitResult(
        bool IsFinished);

    public record GetNextFlashcard(
        int ChallengeId) : IRequest<FlashcardItem>;

    public class GetNextFlashcardHandler : IRequestHandler<GetNextFlashcard, FlashcardItem>
    {
        private readonly MemodexContext _memodexContext;

        public GetNextFlashcardHandler(
            MemodexContext memodexContext)
            => _memodexContext = memodexContext;

        public async Task<FlashcardItem> Handle(
            GetNextFlashcard request,
            CancellationToken cancellationToken)
        {
            Challenge? challenge = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps
                    .Where(step => step.NeedsReview)
                    .OrderBy(step => step.Index))
                .Where(challenge => challenge.Id == request.ChallengeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge is null)
            {
                throw new InvalidOperationException("Could not find challenge");
            }

            if (challenge.CurrentStepIndex is null)
            {
                throw new InvalidOperationException("Challenge has no current step");
            }

            if (challenge.State != ChallengeState.InReview)
            {
                throw new InvalidOperationException("Challenge is not in review");
            }

            ChallengeStep? currentStep = challenge.ChallengeSteps
                .FirstOrDefault(step => step.Index == challenge.CurrentStepIndex);

            if (currentStep is null)
            {
                throw new InvalidOperationException("Could not find current step");
            }

            if (!currentStep.NeedsReview)
            {
                throw new InvalidOperationException("Current step does not need review");
            }

            var flashcard = await _memodexContext.Flashcards
                .Include(deck => deck.Deck)
                .Select(item => new
                {
                    item.Id,
                    item.Question,
                    item.Answer,
                    item.Deck.Name,
                    item.Deck.Flashcards.Count
                })
                .FirstOrDefaultAsync(
                    item => item.Id == currentStep.FlashcardId,
                    cancellationToken);

            if (flashcard is null)
            {
                throw new InvalidOperationException("Could not find flashcard for challenge");
            }

            return new FlashcardItem(
                flashcard.Id,
                flashcard.Question,
                flashcard.Answer,
                flashcard.Name,
                flashcard.Count,
                challenge.CurrentStepIndex.Value + 1);
        }
    }

    public class SubmitFlashcardHandler : IRequestHandler<SubmitFlashcard, SubmitResult>
    {
        private readonly MemodexContext _memodexContext;

        public SubmitFlashcardHandler(
            MemodexContext memodexContext)
            => _memodexContext = memodexContext;

        public async Task<SubmitResult> Handle(
            SubmitFlashcard request,
            CancellationToken cancellationToken)
        {
            Challenge? challenge = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps
                    .Where(step => step.NeedsReview)
                    .OrderBy(step => step.Index))
                .FirstOrDefaultAsync(
                    item => item.Id == request.ChallengeId,
                    cancellationToken);

            if (challenge is null)
            {
                throw new InvalidOperationException("Could not find challenge");
            }

            if (challenge.CurrentStepIndex is null)
            {
                throw new InvalidOperationException("Challenge has no current step");
            }

            if (challenge.State != ChallengeState.InReview)
            {
                throw new InvalidOperationException("Challenge is not in review");
            }

            ChallengeStep? currentStep = challenge.ChallengeSteps
                .FirstOrDefault(step => step.Index == challenge.CurrentStepIndex);

            if (currentStep is null)
            {
                throw new InvalidOperationException("Could not find current step");
            }

            ChallengeStep? nextStep = challenge.ChallengeSteps
                .SkipWhile(step => step.Index <= currentStep.Index)
                .FirstOrDefault();

            challenge.CurrentStepIndex = nextStep?.Index;

            if (nextStep is null)
            {
                challenge.State = ChallengeState.Complete;
            }

            challenge.UpdatedAt = DateTime.UtcNow;

            await _memodexContext.SaveChangesAsync(cancellationToken);

            return new SubmitResult(nextStep is null);
        }
    }
}