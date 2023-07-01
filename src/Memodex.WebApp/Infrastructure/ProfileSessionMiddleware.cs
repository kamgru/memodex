using MediatR;

namespace Memodex.WebApp.Infrastructure;

public class ProfileSessionMiddleware
{
    private readonly RequestDelegate _next;

    public ProfileSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        if (context.Request.Path == "/SelectProfile")
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