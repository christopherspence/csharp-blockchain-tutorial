FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
EXPOSE 80
EXPOSE 443

WORKDIR /src
COPY ["BlockChainTutorial.csproj", "BlockChainTutorial/"]
RUN dotnet restore "BlockChainTutorial.csproj"
COPY . . 
WORKDIR "/src/BlockChainTutorial"
RUN dotnet build "BlockChainTutorial.csproj" -c Release -o /app

RUN dotnet publish "BlockChainTutorial.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BlockChainTutorial.dll"]