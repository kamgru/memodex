namespace Memodex.Cli;

public static class InvocationContextExtension
{
    public static T GetOptionValue<T>(
        this InvocationContext invocationContext,
        Option<T> option) =>
        invocationContext.ParseResult.GetValueForOption(option)
        ?? throw new InvalidOperationException($"Option {option.Name} not found");

    public static T GetRequiredService<T>(
        this InvocationContext invocationContext) =>
        invocationContext.BindingContext.GetService(typeof(T)) is T service
            ? service
            : throw new InvalidOperationException($"Service {typeof(T).Name} not found");
}