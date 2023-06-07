FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Passaro.API/Passaro.API.csproj", "Passaro.API/"]
RUN dotnet restore "Passaro.API/Passaro.API.csproj"
COPY . .
WORKDIR "/src/Passaro.API"
RUN dotnet build "Passaro.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Passaro.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Passaro.API.dll"]