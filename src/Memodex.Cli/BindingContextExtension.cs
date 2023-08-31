using Microsoft.Extensions.Configuration;

namespace Memodex.Cli;

public static class BindingContextExtension
{
    public static void AddServices(
        this BindingContext bindingContext)
    {

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();
        
        bindingContext.AddService(_ => configuration);
        bindingContext.AddService<MemodexContextFactory>(_ => new MemodexContextFactory(configuration));
    }
}