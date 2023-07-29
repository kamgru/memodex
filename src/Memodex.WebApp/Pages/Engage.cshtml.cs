using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class Engage : PageModel
{
    private readonly IMediator _mediator;

    public Engage(IMediator mediator) 
        => _mediator = mediator;

    public FlashcardItem? CurrentFlashcard { get; set; }

    [BindProperty] 
    public StepInput? Input { get; set; }

    public async Task<IActionResult> OnGetAsync(int challengeId)
    {
        CurrentFlashcard = await _mediator.Send(new GetNextFlashcard(challengeId));
        Input = new StepInput(
            challengeId, 
            CurrentFlashcard.Id, 
            false);
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
            Input.FlashcardId,
            Input.NeedsReview));

        return result.IsFinished
            ? RedirectToPage("CompleteChallenge")
            : RedirectToPage("Engage", new { challengeId = Input.ChallengeId });
    }

    public record StepInput(int ChallengeId, int FlashcardId, bool NeedsReview);

    public record FlashcardItem(int Id, string Question, string Answer);

    public record FlashcardStep(FlashcardItem Flashcard, bool IsLast);

    public record GetNextFlashcard(int ChallengeId) : IRequest<FlashcardItem>;

    public record SubmitFlashcard(int ChallengeId, int FlashcardId, bool NeedsReview) : IRequest<SubmitResult>;

    public record SubmitResult(bool IsFinished);

    public class GetNextFlashcardHandler : IRequestHandler<GetNextFlashcard, FlashcardItem>
    {
        private readonly MemodexContext _memodexContext;

        public GetNextFlashcardHandler(MemodexContext memodexContext) 
            => _memodexContext = memodexContext;

        public async Task<FlashcardItem> Handle(
            GetNextFlashcard request, 
            CancellationToken cancellationToken)
        {
            Challenge? challenge = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps.OrderBy(step => step.Index))
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
            
            if (challenge.IsFinished)
            {
                throw new InvalidOperationException("Challenge is already finished");
            }
            
            ChallengeStep? currentStep = challenge.ChallengeSteps
                .FirstOrDefault(step => step.Index == challenge.CurrentStepIndex);
            
            if (currentStep is null)
            {
                throw new InvalidOperationException("Could not find current step");
            }
            
            Flashcard? flashcard = await _memodexContext.Flashcards
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
                flashcard.Answer);
        }
    }

    public class SubmitFlashcardHandler : IRequestHandler<SubmitFlashcard, SubmitResult>
    {
        private readonly MemodexContext _memodexContext;

        public SubmitFlashcardHandler(MemodexContext memodexContext) 
            => _memodexContext = memodexContext;

        public async Task<SubmitResult> Handle(
            SubmitFlashcard request, 
            CancellationToken cancellationToken)
        {
            Challenge? challenge = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps.OrderBy(step => step.Index))
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

            if (challenge.IsFinished)
            {
                throw new InvalidOperationException("Challenge is already finished");
            }
            
            ChallengeStep? currentStep = challenge.ChallengeSteps
                .FirstOrDefault(step => step.Index == challenge.CurrentStepIndex);
            
            if (currentStep is null)
            {
                throw new InvalidOperationException("Could not find current step");
            }
            
            currentStep.NeedsReview = request.NeedsReview;
            
            if (challenge.CurrentStepIndex == challenge.ChallengeSteps.Count - 1)
            {
                challenge.IsFinished = true;
            }
            else
            {
                challenge.CurrentStepIndex++;
            }
            
            await _memodexContext.SaveChangesAsync(cancellationToken);

            return new SubmitResult(challenge.IsFinished);
        }
    }
}