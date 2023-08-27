FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Memodex.WebApp/Memodex.WebApp.csproj", "Memodex.WebApp/"]
RUN dotnet restore "Memodex.WebApp/Memodex.WebApp.csproj"
COPY src/ .
WORKDIR "Memodex.WebApp"
RUN dotnet build "Memodex.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Memodex.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS data
WORKDIR /app
RUN mkdir -p /app/data
COPY data/ /app/data/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=data /app/data data/
ENTRYPOINT ["dotnet", "Memodex.WebApp.dll"]
