FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Memodex.Cli/Memodex.Cli.csproj", "Memodex.Cli/"]
COPY ["src/Memodex.DataAccess/Memodex.DataAccess.csproj", "Memodex.DataAccess/"]
RUN dotnet restore "Memodex.Cli/Memodex.Cli.csproj"
COPY src/ .
WORKDIR "/src/Memodex.Cli"
RUN dotnet build "Memodex.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Memodex.Cli.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mdx.dll"]
