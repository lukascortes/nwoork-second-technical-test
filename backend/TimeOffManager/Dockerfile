FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["TimeOffManager.csproj", "./"]
RUN dotnet restore "./TimeOffManager.csproj"

COPY . .
RUN dotnet publish "TimeOffManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY timeoff.db ./timeoff.db

ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "TimeOffManager.dll"]
