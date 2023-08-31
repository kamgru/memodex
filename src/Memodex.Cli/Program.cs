Command rootCommand = new RootCommand();

CommandLineBuilder builder = new(rootCommand);
builder.AddMiddleware((
        context,
        next) =>
    {
        context.BindingContext.AddServices();
        return next(context);
    })
    .UseDefaults();

rootCommand.Add(new InitCommand());
rootCommand.Add(new CategoryCommand());
rootCommand.Add(new DeckCommand());
rootCommand.Add(new DbCommand());

builder.Build().InvokeAsync(args);