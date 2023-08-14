using MediatR;
using Memodex.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Pages;

public class MyProfile : PageModel
{
    private readonly IMediator _mediator;

    public MyProfile(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPostUpdateThemeAsync([FromBody] UpdateTheme updateTheme)
    {
        await _mediator.Send(updateTheme);
        return new OkResult();
    }

    public record UpdateTheme(int ProfileId, string Theme) : IRequest;
    
    public class UpdateThemeHandler : IRequestHandler<UpdateTheme>
    {
        private readonly MemodexContext _memodexContext;
        
        public UpdateThemeHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }
        
        public async Task Handle(UpdateTheme request, CancellationToken cancellationToken)
        {
            Profile? profile = await _memodexContext.Profiles.FirstOrDefaultAsync(
                item => item.Id == request.ProfileId,
                cancellationToken);
            
            if (profile == null)
            {
                throw new InvalidOperationException("Profile not found.");
            }
            
            profile.PreferredTheme = request.Theme;
            
            await _memodexContext.SaveChangesAsync(cancellationToken);
        }
    }
}