using MediatR;
using Memodex.DataAccess;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class CreateProfile : PageModel
{
    private readonly IMediator _mediator;

    public CreateProfile(
        IMediator mediator)
    {
        _mediator = mediator;
        FormData = new CreateProfileRequest("Memodexer", "default.png");
    }

    [BindProperty]
    public CreateProfileRequest FormData { get; set; }

    public List<AvatarImage> Avatars { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Avatars = await _mediator.Send(new GetAvatarsRequest());
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _mediator.Send(FormData);

        return RedirectToPage("SelectProfile");
    }

    public record AvatarImage(
        int Id,
        string ImageUrl,
        string ImageName);

    public record GetAvatarsRequest : IRequest<List<AvatarImage>>;

    public record CreateProfileRequest(
        string Name,
        string Image) : IRequest;

    public class CreateProfileHandler : IRequestHandler<CreateProfileRequest>
    {
        private readonly MemodexContext _memodexContext;

        public CreateProfileHandler(
            MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }

        public Task Handle(
            CreateProfileRequest request,
            CancellationToken cancellationToken)
        {
            _memodexContext.Profiles.Add(new Profile
            {
                Name = request.Name,
                AvatarPath = request.Image
            });

            return _memodexContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class GetAvatarsHandler : IRequestHandler<GetAvatarsRequest, List<AvatarImage>>
    {
        private readonly MediaPathProvider _mediaPathProvider;
        private readonly MemodexContext _memodexContext;

        public GetAvatarsHandler(
            MediaPathProvider mediaPathProvider,
            MemodexContext memodexContext)
        {
            _mediaPathProvider = mediaPathProvider;
            _memodexContext = memodexContext;
        }

        public async Task<List<AvatarImage>> Handle(
            GetAvatarsRequest request,
            CancellationToken cancellationToken)
        {
            List<Avatar> avatars = await _memodexContext.Avatars.ToListAsync(cancellationToken);
            return avatars.Select(
                avatar => new AvatarImage(
                    avatar.Id,
                    _mediaPathProvider.GetAvatarThumbnailPath(avatar.ImageFilename),
                    avatar.ImageFilename))
                .ToList();
        }
    }
}