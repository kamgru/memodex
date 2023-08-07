using System.Collections.Immutable;
using MediatR;

namespace Memodex.WebApp.Infrastructure;

public class ProfileSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ImmutableHashSet<string> _allowedPaths = ImmutableHashSet.Create(
        "/editprofile",
        "/createprofile",
        "/selectprofile"
    );
    
    public ProfileSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        if (_allowedPaths.Contains(context.Request.Path.ToString().ToLowerInvariant()))
        {
            await _next(context);
            return;
        }
        
        int? profileId = context.Session.GetInt32(Common.Constants.SelectedProfileSessionKey);
        if (profileId == null)
        {
            context.Response.Redirect("/SelectProfile");
        }
        else
        {
            await _next(context);
        }
    }
}