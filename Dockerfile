FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:18 AS node
WORKDIR /node
COPY ["src/Memodex.WebApp/package.json", "src/Memodex.WebApp/package-lock.json", "./"]
RUN npm install
COPY ["src/Memodex.WebApp/", "./"]
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Memodex.WebApp/Memodex.WebApp.csproj", "Memodex.WebApp/"]
RUN dotnet restore "Memodex.WebApp/Memodex.WebApp.csproj"
COPY src/ .
WORKDIR "Memodex.WebApp"
COPY --from=node ./node/wwwroot/ ./wwwroot/

RUN dotnet build "Memodex.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Memodex.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS data
WORKDIR /app
COPY data/ /app/data

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=data /app/data data/
ENTRYPOINT ["dotnet", "Memodex.WebApp.dll"]
