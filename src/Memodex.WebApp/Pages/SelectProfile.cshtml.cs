using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class SelectProfile : PageModel
{
    private readonly IMediator _mediator;

    public SelectProfile(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ProfileItem> Profiles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Profiles = await _mediator.Send(new GetProfilesRequest());
        if (Profiles.Count == 0)
        {
            return RedirectToPage("CreateProfile");
        } 
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ProfileItem profileItem)
    {
        await _mediator.Send(new SelectProfileRequest
        {
            ProfileId = profileItem.Id
        });
        return RedirectToPage("Index");
    }
    
    public class GetProfilesRequest : IRequest<List<ProfileItem>>
    {
    }

    public class SelectProfileRequest : IRequest
    {
        public int ProfileId { get; set; }
    }

    public class ProfileItem
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string AvatarPath { get; set; }
    }
    
    public class GetProfilesHandler : IRequestHandler<GetProfilesRequest, List<ProfileItem>>
    {
        private readonly MemodexContext _memodexContext;
        private readonly IConfiguration _configuration;

        public GetProfilesHandler(MemodexContext memodexContext, IConfiguration configuration)
        {
            _memodexContext = memodexContext;
            _configuration = configuration;
        }

        public async Task<List<ProfileItem>> Handle(GetProfilesRequest request, CancellationToken cancellationToken)
        {
            string? rootPath = _configuration.GetSection("Media")
                .GetSection("Avatars")
                .GetValue<string>("Path");

            if (string.IsNullOrEmpty(rootPath))
            {
                throw new InvalidOperationException("Missing configuration for Media:Avatars:Path.");
            }
            
            return await _memodexContext.Profiles
                .Select(item => new ProfileItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    AvatarPath = Path.Combine("media", rootPath, $"t_{item.AvatarPath}")
                })
                .ToListAsync(cancellationToken);
        }
    }
    
    public class SelectProfileHandler : IRequestHandler<SelectProfileRequest>
    {
        private readonly MemodexContext _memodexContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SelectProfileHandler(MemodexContext memodexContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _memodexContext = memodexContext;
        }

        public async Task Handle(SelectProfileRequest request, CancellationToken cancellationToken)
        {
            Profile? profile = await _memodexContext.Profiles.FindAsync(request.ProfileId);
            if (profile == null)
            {
                throw new InvalidOperationException("Profile not found.");
            }

            if (_httpContextAccessor.HttpContext is null)
            {
                throw new InvalidOperationException("Could not access HttpContext.");
            }
            
            _httpContextAccessor.HttpContext.Session.SetInt32(Common.Constants.SelectedProfileSessionKey, profile.Id);
        }
    }
}