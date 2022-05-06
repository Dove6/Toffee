FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Toffee/Toffee.csproj", "Toffee/"]
RUN dotnet restore "Toffee/Toffee.csproj"
COPY . .
WORKDIR "/src/Toffee"
RUN dotnet build "Toffee.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Toffee.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Toffee.dll"]

# TODO: make sure .dockerignore stays up to date (and this file too >.<)
