using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class CompleteChallenge : PageModel
{
    private readonly IMediator _mediator;

    public CompleteChallenge(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public ChallengeItem? CompletedChallenge { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        CompletedChallenge = await _mediator.Send(new GetChallenge(challengeId));
        return Page();
    }

    public record ChallengeItem(
        int Id,
        string Title,
        ChallengeState State);

    public record GetChallenge(
        int Id) : IRequest<ChallengeItem>;

    public class GetChallengeHandler : IRequestHandler<GetChallenge, ChallengeItem>
    {
        private readonly MemodexContext _memodexContext;

        public GetChallengeHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public async Task<ChallengeItem> Handle(
            GetChallenge request,
            CancellationToken cancellationToken)
        {
            Challenge? challenge = await _memodexContext.Challenges
                .Include(item => item.Deck)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken: cancellationToken);

            if (challenge is null)
            {
                throw new InvalidOperationException($"Challenge {request.Id} not found");
            }

            if (challenge.State == ChallengeState.InProgress)
            {
                throw new InvalidOperationException($"Challenge {request.Id} is in progress");
            }

            return new ChallengeItem(
                challenge.Id,
                challenge.Deck.Name,
                challenge.State);
        }
    }
}