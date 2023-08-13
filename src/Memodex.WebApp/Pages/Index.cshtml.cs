using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IProfileProvider _profileProvider;

    public IndexModel(
        IMediator mediator,
        IProfileProvider profileProvider)
    {
        _mediator = mediator;
        _profileProvider = profileProvider;
    }

    public async Task<IActionResult> OnGet()
    {
        int? profileId = _profileProvider.GetCurrentProfileId()
            ?? throw new InvalidOperationException("No profile is currently selected.");
        
        Challenges = await _mediator.Send(new GetPastChallenges(profileId.Value));
        return Page();
    }

    public PastChallenges? Challenges { get; set; }
    
    public record PastChallenges(
        List<UnfinishedChallenge> UnfinishedChallenges,
        List<InReviewChallenge> InReviewChallenges);
    
    public record UnfinishedChallenge(
        int Id,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string DeckName,
        int CurrentStep,
        int TotalSteps);
    
    public record InReviewChallenge(
        int Id,
        DateTime CreatedOn,
        string DeckName,
        int StepsToReview);
    
    public record GetPastChallenges(
        int ProfileId) : IRequest<PastChallenges>;
    
    public class GetPastChallengesHandler : IRequestHandler<GetPastChallenges, PastChallenges>
    {
        private readonly MemodexContext _memodexContext;

        public GetPastChallengesHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<PastChallenges> Handle(
            GetPastChallenges request,
            CancellationToken cancellationToken)
        {
            List<UnfinishedChallenge> unfinishedChallenges = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps)
                .Include(challenge => challenge.Deck)
                .Where(challenge => challenge.ProfileId == request.ProfileId)
                .Where(challenge => challenge.State == ChallengeState.InProgress)
                .Select(challenge => new UnfinishedChallenge(
                    challenge.Id,
                    challenge.CreatedAt,
                    challenge.UpdatedAt,
                    challenge.Deck.Name,
                    challenge.CurrentStepIndex.HasValue ? challenge.CurrentStepIndex.Value + 1 : 0,
                    challenge.ChallengeSteps.Count))
                .ToListAsync(cancellationToken);

            List<InReviewChallenge> inReviewChallenges = await _memodexContext.Challenges
                .Include(challenge => challenge.ChallengeSteps)
                .Include(challenge => challenge.Deck)
                .Where(challenge => challenge.ProfileId == request.ProfileId)
                .Where(challenge => challenge.State == ChallengeState.InReview)
                .Select(challenge => new InReviewChallenge(
                    challenge.Id,
                    challenge.CreatedAt,
                    challenge.Deck.Name,
                    challenge.ChallengeSteps.Count(step => step.NeedsReview)))
                .ToListAsync(cancellationToken);

            return new PastChallenges(
                unfinishedChallenges,
                inReviewChallenges);
        }
    }
}