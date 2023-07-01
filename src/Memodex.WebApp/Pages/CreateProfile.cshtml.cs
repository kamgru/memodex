using MediatR;
using Memodex.WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class CreateProfile : PageModel
{
    private readonly IMediator _mediator;
    
    public CreateProfile(IMediator mediator)
    {
        _mediator = mediator;
        FormData = new CreateProfileRequest
        {
            Name = "NewProfile"
        };
    }
    
    [BindProperty]
    public CreateProfileRequest FormData { get; set; } 
    
    public IActionResult OnGet()
    {
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
    
    public class CreateProfileRequest : IRequest
    {
        public required string Name { get; set; }
    }
    
    public class CreateProfileHandler : IRequestHandler<CreateProfileRequest>
    {
        private readonly MemodexContext _memodexContext;
        
        public CreateProfileHandler(MemodexContext memodexContext)
        {
            _memodexContext = memodexContext;
        }
        
        public Task Handle(CreateProfileRequest request, CancellationToken cancellationToken)
        { 
            _memodexContext.Profiles.Add(new Profile
            {
                Name = request.Name
            });
            
            return _memodexContext.SaveChangesAsync(cancellationToken);    
        }
    }
}